using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Commands.ChangeMemberRole;

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
