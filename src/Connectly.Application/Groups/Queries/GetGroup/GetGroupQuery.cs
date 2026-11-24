using Connectly.Application.Groups.Models;
using MediatR;

namespace Connectly.Application.Groups.Queries.GetGroup;

// ---------- details (name/description/member count are visible even for private groups) ----------
public record GetGroupQuery(Guid GroupId) : IRequest<GroupDetailsDto>;
