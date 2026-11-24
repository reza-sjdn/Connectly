using Connectly.Application.Auth.Commands.Login;
using Connectly.Application.Auth.Commands.Logout;
using Connectly.Application.Auth.Commands.RefreshToken;
using Connectly.Application.Auth.Commands.Register;
using Connectly.Application.Auth.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Connectly.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterCommand command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenCommand command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return NoContent();
    }
}
