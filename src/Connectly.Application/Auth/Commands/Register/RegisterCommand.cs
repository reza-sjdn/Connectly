using Connectly.Application.Auth.Models;
using MediatR;

namespace Connectly.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<AuthResponse>;
