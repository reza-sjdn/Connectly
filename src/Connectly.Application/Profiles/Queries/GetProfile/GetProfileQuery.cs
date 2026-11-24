using Connectly.Application.Profiles.Dtos;
using MediatR;

namespace Connectly.Application.Profiles.Queries.GetProfile;

public record GetProfileQuery(Guid UserId) : IRequest<PublicProfileDto>;
