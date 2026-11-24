using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using MediatR;

namespace Connectly.Application.Groups.Queries.GetGroup;

public class GetGroupQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupQuery, GroupDetailsDto>
{
    public Task<GroupDetailsDto> Handle(GetGroupQuery request, CancellationToken ct)
        => GroupProjection.DetailsAsync(db, request.GroupId, currentUser.UserId, ct);
}
