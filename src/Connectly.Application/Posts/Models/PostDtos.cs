using Connectly.Application.Common.Models;

namespace Connectly.Application.Posts.Models;

public record PostDto(
    Guid Id, AuthorDto Author, string? Content, string? ImageUrl, DateTime CreatedAtUtc,
    int LikesCount, int CommentsCount, bool IsLikedByMe, bool IsMine,
    Guid? GroupId, string? GroupName);

public record CommentDto(
    Guid Id, Guid PostId, AuthorDto Author, string Content, DateTime CreatedAtUtc,
    bool IsMine, bool CanDelete);

public record LikeStateDto(bool IsLiked, int LikesCount);
