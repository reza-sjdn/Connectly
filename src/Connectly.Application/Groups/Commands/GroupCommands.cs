using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Commands;

// ---------- create / update ----------
public record CreateGroupCommand(string Name, string? Description, GroupPrivacy Privacy) : IRequest<GroupDetailsDto>;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Privacy).IsInEnum();
    }
}

public class CreateGroupCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<CreateGroupCommand, GroupDetailsDto>
{
    public async Task<GroupDetailsDto> Handle(CreateGroupCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        var group = new Group
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Privacy = request.Privacy,
            CreatedById = me
        };
        db.Groups.Add(group);
        db.GroupMembers.Add(new GroupMember
        {
            GroupId = group.Id, UserId = me, Role = GroupRole.Admin, Status = MembershipStatus.Active
        });
        await db.SaveChangesAsync(ct);

        return await GroupProjection.DetailsAsync(db, group.Id, me, ct);
    }
}

public record UpdateGroupCommand(Guid GroupId, string Name, string? Description, GroupPrivacy Privacy) : IRequest<GroupDetailsDto>;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Privacy).IsInEnum();
    }
}

public class UpdateGroupCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpdateGroupCommand, GroupDetailsDto>
{
    public async Task<GroupDetailsDto> Handle(UpdateGroupCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireAdminAsync(db, request.GroupId, me, ct);
        var group = await GroupAccess.GetGroupAsync(db, request.GroupId, ct);

        var becamePublic = group.Privacy == GroupPrivacy.Private && request.Privacy == GroupPrivacy.Public;

        group.Name = request.Name.Trim();
        group.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        group.Privacy = request.Privacy;
        await db.SaveChangesAsync(ct);

        // Nobody needs approval for a public group, so pending requests are accepted.
        if (becamePublic)
            await db.GroupMembers
                .Where(m => m.GroupId == group.Id && m.Status == MembershipStatus.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.Status, MembershipStatus.Active), ct);

        return await GroupProjection.DetailsAsync(db, group.Id, me, ct);
    }
}

// ---------- join / leave ----------
public record JoinGroupCommand(Guid GroupId) : IRequest<GroupDetailsDto>;

public class JoinGroupCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications)
    : IRequestHandler<JoinGroupCommand, GroupDetailsDto>
{
    public async Task<GroupDetailsDto> Handle(JoinGroupCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var group = await GroupAccess.GetGroupAsync(db, request.GroupId, ct);

        // Idempotent: already a member (or already requested) -> just return the current state.
        if (await GroupAccess.MembershipAsync(db, group.Id, me, ct) is null)
        {
            var isPublic = group.Privacy == GroupPrivacy.Public;
            db.GroupMembers.Add(new GroupMember
            {
                GroupId = group.Id, UserId = me, Role = GroupRole.Member,
                Status = isPublic ? MembershipStatus.Active : MembershipStatus.Pending
            });

            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateException) { return await GroupProjection.DetailsAsync(db, group.Id, me, ct); }   // double click

            if (!isPublic)
            {
                var adminIds = await db.GroupMembers
                    .Where(m => m.GroupId == group.Id && m.Role == GroupRole.Admin && m.Status == MembershipStatus.Active)
                    .Select(m => m.UserId).ToListAsync(ct);
                foreach (var adminId in adminIds)
                    await notifications.PublishGroupAsync(adminId, me, NotificationType.GroupJoinRequested, group.Id, ct);
            }
        }

        return await GroupProjection.DetailsAsync(db, group.Id, me, ct);
    }
}

/// <summary>Leave the group, or cancel my pending join request.</summary>
public record LeaveGroupCommand(Guid GroupId) : IRequest;

public class LeaveGroupCommandHandler(IApplicationDbContext db, ICurrentUser currentUser) : IRequestHandler<LeaveGroupCommand>
{
    public async Task Handle(LeaveGroupCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var membership = await GroupAccess.MembershipAsync(db, request.GroupId, me, ct);
        if (membership is null) return;

        if (membership is { Status: MembershipStatus.Active, Role: GroupRole.Admin })
        {
            var otherAdmins = await db.GroupMembers.CountAsync(m => m.GroupId == request.GroupId && m.UserId != me
                && m.Role == GroupRole.Admin && m.Status == MembershipStatus.Active, ct);
            var otherMembers = await db.GroupMembers.CountAsync(m => m.GroupId == request.GroupId && m.UserId != me
                && m.Status == MembershipStatus.Active, ct);

            if (otherAdmins == 0 && otherMembers > 0)
                throw new ValidationException([new ValidationFailure("GroupId", "Make another member an admin before you leave.")]);
        }

        db.GroupMembers.Remove(membership);
        await db.SaveChangesAsync(ct);
    }
}

// ---------- admin: join requests ----------
public record ReviewJoinRequestCommand(Guid GroupId, Guid UserId, bool Approve) : IRequest;

public class ReviewJoinRequestCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications)
    : IRequestHandler<ReviewJoinRequestCommand>
{
    public async Task Handle(ReviewJoinRequestCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireAdminAsync(db, request.GroupId, me, ct);

        var pending = await db.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == request.GroupId
            && m.UserId == request.UserId && m.Status == MembershipStatus.Pending, ct)
            ?? throw new NotFoundException("Join request", request.UserId);

        if (request.Approve)
        {
            pending.Status = MembershipStatus.Active;
            await db.SaveChangesAsync(ct);
            await notifications.PublishGroupAsync(request.UserId, me, NotificationType.GroupRequestApproved, request.GroupId, ct);
        }
        else
        {
            db.GroupMembers.Remove(pending);
            await db.SaveChangesAsync(ct);
        }
    }
}

// ---------- admin: roles and removal ----------
public record ChangeMemberRoleCommand(Guid GroupId, Guid UserId, GroupRole Role) : IRequest;

public class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator() => RuleFor(x => x.Role).IsInEnum();
}

public class ChangeMemberRoleCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<ChangeMemberRoleCommand>
{
    public async Task Handle(ChangeMemberRoleCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireAdminAsync(db, request.GroupId, me, ct);

        if (request.UserId == me)
            throw new ValidationException([new ValidationFailure("UserId", "You cannot change your own role.")]);

        var member = await db.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == request.GroupId
            && m.UserId == request.UserId && m.Status == MembershipStatus.Active, ct)
            ?? throw new NotFoundException("Group member", request.UserId);

        member.Role = request.Role;
        await db.SaveChangesAsync(ct);
    }
}

public record RemoveMemberCommand(Guid GroupId, Guid UserId) : IRequest;

public class RemoveMemberCommandHandler(IApplicationDbContext db, ICurrentUser currentUser) : IRequestHandler<RemoveMemberCommand>
{
    public async Task Handle(RemoveMemberCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireAdminAsync(db, request.GroupId, me, ct);

        if (request.UserId == me)
            throw new ValidationException([new ValidationFailure("UserId", "Use \"Leave group\" to remove yourself.")]);

        var member = await db.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == request.GroupId
            && m.UserId == request.UserId && m.Status == MembershipStatus.Active, ct)
            ?? throw new NotFoundException("Group member", request.UserId);

        if (member.Role == GroupRole.Admin)
            throw new ValidationException([new ValidationFailure("UserId", "Demote this admin to member before removing them.")]);

        db.GroupMembers.Remove(member);
        await db.SaveChangesAsync(ct);
    }
}

// ---------- group chat ----------
public record SendGroupMessageCommand(Guid GroupId, string Content) : IRequest<GroupMessageDto>;

public class SendGroupMessageCommandValidator : AbstractValidator<SendGroupMessageCommand>
{
    public SendGroupMessageCommandValidator() => RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
}

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
                new Common.Models.AuthorDto(m.Sender.Id, m.Sender.FirstName, m.Sender.LastName, m.Sender.AvatarUrl),
                m.Content, m.CreatedAtUtc))
            .FirstAsync(ct);

        var memberIds = await db.GroupMembers
            .Where(m => m.GroupId == request.GroupId && m.Status == MembershipStatus.Active)
            .Select(m => m.UserId).ToListAsync(ct);

        await realtime.GroupMessageReceivedAsync(memberIds, dto, ct);   // sender's other tabs included
        return dto;
    }
}
