using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Chat.Commands.MarkConversationRead;

public class MarkConversationReadCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IRealtimeNotifier realtime, TimeProvider clock)
    : IRequestHandler<MarkConversationReadCommand>
{
    public async Task Handle(MarkConversationReadCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var (a, b) = ChatPair.Order(me, request.OtherUserId);

        var conversationId = await db.Conversations
            .Where(c => c.UserAId == a && c.UserBId == b)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);
        if (conversationId is null) return;

        var now = clock.GetUtcNow().UtcDateTime;
        var updated = await db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderId == request.OtherUserId && m.ReadAtUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.ReadAtUtc, now), ct);

        if (updated > 0) await realtime.MessagesReadAsync(request.OtherUserId, me, now, ct);
    }
}
