using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Comments.Commands.AddComment;

public record AddCommentCommand(Guid PostId, string Content) : IRequest<CommentDto>;
