using Connectly.Application.Chat.Models;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Chat.Commands;

public record SendMessageCommand(Guid RecipientId, string Content) : IRequest<MessageDto>;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator() => RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
}

public class SendMessageCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IRealtimeNotifier realtime, TimeProvider clock)
    : IRequestHandler<SendMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        if (me == request.RecipientId)
            throw new ValidationException([new ValidationFailure("RecipientId", "You cannot message yourself.")]);

        if (!await db.UserProfiles.AnyAsync(p => p.Id == request.RecipientId, ct))
            throw new NotFoundException(nameof(UserProfile), request.RecipientId);

        var (a, b) = ChatPair.Order(me, request.RecipientId);
        var now = clock.GetUtcNow().UtcDateTime;

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.UserAId == a && c.UserBId == b, ct);
        if (conversation is null)
        {
            // Two first messages at the exact same instant would hit the unique index; extremely rare, surfaced as an error.
            conversation = new Conversation { UserAId = a, UserBId = b, CreatedAtUtc = now };
            db.Conversations.Add(conversation);
        }
        conversation.LastMessageAtUtc = now;

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = me,
            Content = request.Content.Trim(),
            CreatedAtUtc = now
        };
        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);

        var dto = new MessageDto(message.Id, conversation.Id, me, request.RecipientId, message.Content, now, null);

        // Both participants: the recipient's tabs get the message, the sender's other tabs stay in sync.
        await realtime.MessageReceivedAsync([me, request.RecipientId], dto, ct);
        return dto;
    }
}

/// <summary>Marks everything the other user sent me as read and tells them ("Seen").</summary>
public record MarkConversationReadCommand(Guid OtherUserId) : IRequest;

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
