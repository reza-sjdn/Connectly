using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Commands.SendGroupMessage;

public class SendGroupMessageCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IRealtimeNotifier realtime)
    : IRequestHandler<SendGroupMessageCommand, GroupMessageDto>
{
    public async Task<GroupMessageDto> Handle(SendGroupMessageCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireMemberAsync(db, request.GroupId, me, ct);

        var message = new GroupMessage { GroupId = request.GroupId, SenderId = me, Content = request.Content.Trim() };
        db.GroupMessages.Add(message);
        await db.SaveChangesAsync(ct);

        var dto = await db.GroupMessages.AsNoTracking().Where(m => m.Id == message.Id)
            .Select(m => new GroupMessageDto(m.Id, m.GroupId,
                new AuthorDto(m.Sender.Id, m.Sender.FirstName, m.Sender.LastName, m.Sender.AvatarUrl),
                m.Content, m.CreatedAtUtc))
            .FirstAsync(ct);

        var memberIds = await db.GroupMembers
            .Where(m => m.GroupId == request.GroupId && m.Status == MembershipStatus.Active)
            .Select(m => m.UserId).ToListAsync(ct);

        await realtime.GroupMessageReceivedAsync(memberIds, dto, ct);   // sender's other tabs included
        return dto;
    }
}
