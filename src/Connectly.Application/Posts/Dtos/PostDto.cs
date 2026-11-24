using Connectly.Application.Common.Dtos;

namespace Connectly.Application.Posts.Dtos;

public record PostDto(
    Guid Id, AuthorDto Author, string? Content, string? ImageUrl, DateTime CreatedAtUtc,
    int LikesCount, int CommentsCount, bool IsLikedByMe, bool IsMine,
    Guid? GroupId, string? GroupName);
