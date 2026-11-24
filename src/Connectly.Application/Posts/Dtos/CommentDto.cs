using Connectly.Application.Common.Dtos;

namespace Connectly.Application.Posts.Dtos;

public record CommentDto(
    Guid Id, Guid PostId, AuthorDto Author, string Content, DateTime CreatedAtUtc,
    bool IsMine, bool CanDelete);
