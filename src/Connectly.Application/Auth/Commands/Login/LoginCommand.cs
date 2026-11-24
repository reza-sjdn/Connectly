using Connectly.Application.Auth.Models;
using MediatR;

namespace Connectly.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
