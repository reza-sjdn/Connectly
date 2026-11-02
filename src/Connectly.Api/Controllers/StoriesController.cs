using Connectly.Application.Common.Models;
using Connectly.Application.Stories.Commands;
using Connectly.Application.Stories.Models;
using Connectly.Application.Stories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connectly.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/stories")]
public class StoriesController(ISender sender) : ControllerBase
{
    private const int UploadLimitBytes = 6 * 1024 * 1024;

    // multipart/form-data: either "image" (file) or "text" (+ optional "backgroundColor" like #1877f2)
    [HttpPost]
    [RequestSizeLimit(UploadLimitBytes)]
    public async Task<ActionResult<StoryDto>> Create(
        [FromForm] string? text, [FromForm] string? backgroundColor, IFormFile? image, CancellationToken ct)
    {
        var upload = image is null ? null : new FileUpload(image.OpenReadStream(), image.FileName, image.ContentType, image.Length);
        return Ok(await sender.Send(new CreateStoryCommand(text, backgroundColor, upload), ct));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StoryGroupDto>>> Feed(CancellationToken ct)
        => Ok(await sender.Send(new GetStoryFeedQuery(), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteStoryCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> MarkViewed(Guid id, CancellationToken ct)
    {
        await sender.Send(new MarkStoryViewedCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/viewers")]
    public async Task<ActionResult<IReadOnlyList<StoryViewerDto>>> Viewers(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetStoryViewersQuery(id), ct));
}
