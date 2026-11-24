using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using Connectly.Domain.Entities.Groups;
using Connectly.Domain.Entities.Groups.Enums;
using MediatR;

namespace Connectly.Application.Groups.Commands.CreateGroup;

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
            GroupId = group.Id,
            UserId = me,
            Role = GroupRole.Admin,
            Status = MembershipStatus.Active
        });
        await db.SaveChangesAsync(ct);

        return await GroupProjection.DetailsAsync(db, group.Id, me, ct);
    }
}
