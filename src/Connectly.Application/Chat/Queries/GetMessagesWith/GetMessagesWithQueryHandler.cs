using Connectly.Application.Chat.Dtos;
using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Chat.Queries.GetMessagesWith;

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
