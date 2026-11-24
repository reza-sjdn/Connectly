using Connectly.Application.Common.Dtos;
using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Comments.Queries.GetComments;

/// <summary>Oldest first, so new comments append at the end and page offsets stay stable.</summary>
public record GetCommentsQuery(Guid PostId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<CommentDto>>;
