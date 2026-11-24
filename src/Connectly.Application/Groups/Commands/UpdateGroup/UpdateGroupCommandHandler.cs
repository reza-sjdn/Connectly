using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Commands.UpdateGroup;

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
