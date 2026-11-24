using Connectly.Application.Auth.Models;
using MediatR;

namespace Connectly.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
