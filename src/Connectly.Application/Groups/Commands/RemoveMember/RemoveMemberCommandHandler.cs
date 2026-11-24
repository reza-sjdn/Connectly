using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Commands.RemoveMember;

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
