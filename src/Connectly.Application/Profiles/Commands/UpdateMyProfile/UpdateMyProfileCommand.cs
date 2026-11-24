using Connectly.Application.Profiles.Dtos;
using MediatR;

namespace Connectly.Application.Profiles.Commands.UpdateMyProfile;

public record UpdateMyProfileCommand(string FirstName, string LastName, string? Bio, DateOnly? DateOfBirth)
    : IRequest<ProfileDto>;
