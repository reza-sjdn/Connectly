using Connectly.Application.Chat.Dtos;
using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Chat.Queries.GetConversations;

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
