using Connectly.Application.Common.Dtos;
using Connectly.Application.Groups.Models;
using MediatR;

namespace Connectly.Application.Groups.Queries.GetGroupMembers;

// ---------- members ----------
public record GetGroupMembersQuery(Guid GroupId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<GroupMemberDto>>;
