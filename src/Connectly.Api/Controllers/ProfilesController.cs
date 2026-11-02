using Connectly.Application.Common.Models;
using Connectly.Application.Profiles.Commands;
using Connectly.Application.Profiles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connectly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profiles")]
public class ProfilesController(ISender sender) : ControllerBase
{
    private const int UploadLimitBytes = 6 * 1024 * 1024;

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> Me(CancellationToken ct)
        => Ok(await sender.Send(new GetMyProfileQuery(), ct));

    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> UpdateMe(UpdateMyProfileCommand command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpPost("me/avatar")]
    [RequestSizeLimit(UploadLimitBytes)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
        => Ok(new { url = await sender.Send(new UpdateProfileImageCommand(ProfileImageKind.Avatar, ToUpload(file)), ct) });

    [HttpPost("me/cover")]
    [RequestSizeLimit(UploadLimitBytes)]
    public async Task<IActionResult> UploadCover(IFormFile file, CancellationToken ct)
        => Ok(new { url = await sender.Send(new UpdateProfileImageCommand(ProfileImageKind.Cover, ToUpload(file)), ct) });

    [HttpGet("suggestions")]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> Suggestions([FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await sender.Send(new GetUserSuggestionsQuery(take), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicProfileDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetProfileQuery(id), ct));

    private static FileUpload ToUpload(IFormFile f) => new(f.OpenReadStream(), f.FileName, f.ContentType, f.Length);
}
