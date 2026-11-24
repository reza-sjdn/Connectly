using Connectly.Application.Common.Dtos;
using Connectly.Application.Follows.Enums;
using MediatR;

namespace Connectly.Application.Follows.Queries.GetFollowList;

public record GetFollowListQuery(Guid UserId, FollowDirection Direction, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<UserSummaryDto>>;
