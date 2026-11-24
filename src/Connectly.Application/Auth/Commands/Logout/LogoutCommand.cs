using MediatR;

namespace Connectly.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
