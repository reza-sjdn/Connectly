using Connectly.Application.Groups.Enums;
using Connectly.Application.Groups.Models;
using MediatR;

namespace Connectly.Application.Groups.Queries.GetGroups;

// ---------- list ----------
public record GetGroupsQuery(GroupListScope Scope) : IRequest<IReadOnlyList<GroupSummaryDto>>;
