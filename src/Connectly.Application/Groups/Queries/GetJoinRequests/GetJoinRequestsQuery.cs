using Connectly.Application.Groups.Models;
using MediatR;

namespace Connectly.Application.Groups.Queries.GetJoinRequests;

// ---------- pending join requests (admins only) ----------
public record GetJoinRequestsQuery(Guid GroupId) : IRequest<IReadOnlyList<JoinRequestDto>>;
