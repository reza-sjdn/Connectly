using Connectly.Application.Chat.Models;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Chat.Queries;

// ---------- Inbox ----------
public record GetConversationsQuery : IRequest<IReadOnlyList<ConversationDto>>;

public class GetConversationsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetConversationsQuery, IReadOnlyList<ConversationDto>>
{
    public async Task<IReadOnlyList<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        var rows = await db.Conversations.AsNoTracking()
            .Where(c => c.UserAId == me || c.UserBId == me)
            .OrderByDescending(c => c.LastMessageAtUtc)
            .Take(50)
            .Select(c => new
            {
                c.Id,
                OtherId = c.UserAId == me ? c.UserBId : c.UserAId,
                c.LastMessageAtUtc,
                Last = db.Messages.Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.CreatedAtUtc).Select(m => m.Content).FirstOrDefault(),
                Unread = db.Messages.Count(m => m.ConversationId == c.Id && m.SenderId != me && m.ReadAtUtc == null)
            })
            .ToListAsync(ct);

        var otherIds = rows.Select(r => r.OtherId).ToList();
        var profiles = await db.UserProfiles.AsNoTracking()
            .Where(p => otherIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        return rows
            .Where(r => profiles.ContainsKey(r.OtherId))
            .Select(r =>
            {
                var p = profiles[r.OtherId];
                var preview = r.Last is { Length: > 80 } ? r.Last[..80] + "…" : r.Last;
                return new ConversationDto(r.Id, new AuthorDto(p.Id, p.FirstName, p.LastName, p.AvatarUrl),
                    preview, r.LastMessageAtUtc, r.Unread);
            })
            .ToList();
    }
}

// ---------- Thread with one user (newest first; client reverses for display) ----------
public record GetMessagesWithQuery(Guid UserId, string? Cursor = null, int PageSize = 30) : IRequest<CursorPage<MessageDto>>;

public class GetMessagesWithQueryValidator : AbstractValidator<GetMessagesWithQuery>
{
    public GetMessagesWithQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}

public class GetMessagesWithQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMessagesWithQuery, CursorPage<MessageDto>>
{
    public async Task<CursorPage<MessageDto>> Handle(GetMessagesWithQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var other = request.UserId;
        var (a, b) = ChatPair.Order(me, other);

        var conversationId = await db.Conversations
            .Where(c => c.UserAId == a && c.UserBId == b)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);
        if (conversationId is null) return new CursorPage<MessageDto>([], null);

        var query = db.Messages.AsNoTracking().Where(m => m.ConversationId == conversationId);
        if (PageCursor.TryDecode(request.Cursor, out var at, out var id))
            query = query.Where(m => m.CreatedAtUtc < at || (m.CreatedAtUtc == at && m.Id.CompareTo(id) < 0));

        var rows = await query
            .OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.Id)
            .Take(request.PageSize + 1)
            .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderId,
                m.SenderId == me ? other : me, m.Content, m.CreatedAtUtc, m.ReadAtUtc))
            .ToListAsync(ct);

        var hasMore = rows.Count > request.PageSize;
        var items = hasMore ? rows.Take(request.PageSize).ToList() : rows;
        return new CursorPage<MessageDto>(items, hasMore ? PageCursor.Encode(items[^1].CreatedAtUtc, items[^1].Id) : null);
    }
}

// ---------- Badge ----------
public record GetUnreadMessageCountQuery : IRequest<int>;

public class GetUnreadMessageCountQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetUnreadMessageCountQuery, int>
{
    public Task<int> Handle(GetUnreadMessageCountQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        return db.Messages.CountAsync(m => m.SenderId != me && m.ReadAtUtc == null
                                           && (m.Conversation.UserAId == me || m.Conversation.UserBId == me), ct);
    }
}
