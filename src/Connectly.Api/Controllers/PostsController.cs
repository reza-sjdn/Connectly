using Connectly.Application.Comments.Commands;
using Connectly.Application.Comments.Queries;
using Connectly.Application.Common.Models;
using Connectly.Application.Posts.Commands;
using Connectly.Application.Posts.Models;
using Connectly.Application.Posts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connectly.Api.Controllers;

public record AddCommentRequest(string Content);

[ApiController]
[Authorize]
[Route("api")]
public class PostsController(ISender sender) : ControllerBase
{
    private const int UploadLimitBytes = 6 * 1024 * 1024;

    // multipart/form-data: "content" (text, optional) + "image" (file, optional) - at least one required;
    // "groupId" (optional) publishes the post inside a group you belong to
    [HttpPost("posts")]
    [RequestSizeLimit(UploadLimitBytes)]
    public async Task<ActionResult<PostDto>> Create(
        [FromForm] string? content, [FromForm] Guid? groupId, IFormFile? image, CancellationToken ct)
    {
        var upload = image is null ? null : new FileUpload(image.OpenReadStream(), image.FileName, image.ContentType, image.Length);
        return Ok(await sender.Send(new CreatePostCommand(content, upload, groupId), ct));
    }

    [HttpGet("posts/feed")]
    public async Task<ActionResult<CursorPage<PostDto>>> Feed(
        [FromQuery] string? cursor, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await sender.Send(new GetFeedQuery(cursor, pageSize), ct));

    [HttpGet("users/{userId:guid}/posts")]
    public async Task<ActionResult<CursorPage<PostDto>>> UserPosts(
        Guid userId, [FromQuery] string? cursor, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await sender.Send(new GetUserPostsQuery(userId, cursor, pageSize), ct));

    [HttpGet("posts/{postId:guid}")]
    public async Task<ActionResult<PostDto>> GetById(Guid postId, CancellationToken ct)
        => Ok(await sender.Send(new GetPostQuery(postId), ct));

    [HttpDelete("posts/{postId:guid}")]
    public async Task<IActionResult> Delete(Guid postId, CancellationToken ct)
    {
        await sender.Send(new DeletePostCommand(postId), ct);
        return NoContent();
    }

    [HttpPost("posts/{postId:guid}/like")]
    public async Task<ActionResult<LikeStateDto>> Like(Guid postId, CancellationToken ct)
        => Ok(await sender.Send(new LikePostCommand(postId), ct));

    [HttpDelete("posts/{postId:guid}/like")]
    public async Task<ActionResult<LikeStateDto>> Unlike(Guid postId, CancellationToken ct)
        => Ok(await sender.Send(new UnlikePostCommand(postId), ct));

    [HttpGet("posts/{postId:guid}/comments")]
    public async Task<ActionResult<PagedResult<CommentDto>>> Comments(
        Guid postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await sender.Send(new GetCommentsQuery(postId, page, pageSize), ct));

    [HttpPost("posts/{postId:guid}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid postId, AddCommentRequest body, CancellationToken ct)
        => Ok(await sender.Send(new AddCommentCommand(postId, body.Content), ct));

    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken ct)
    {
        await sender.Send(new DeleteCommentCommand(commentId), ct);
        return NoContent();
    }
}
