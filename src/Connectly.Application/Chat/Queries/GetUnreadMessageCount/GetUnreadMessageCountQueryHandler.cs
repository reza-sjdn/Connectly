using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Chat.Queries.GetUnreadMessageCount;

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
