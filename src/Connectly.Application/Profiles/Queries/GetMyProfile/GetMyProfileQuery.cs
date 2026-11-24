using Connectly.Application.Profiles.Dtos;
using MediatR;

namespace Connectly.Application.Profiles.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<ProfileDto>;
