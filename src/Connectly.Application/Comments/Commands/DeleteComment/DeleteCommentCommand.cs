using MediatR;

namespace Connectly.Application.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(Guid CommentId) : IRequest;
