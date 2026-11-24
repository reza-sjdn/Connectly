using Connectly.Application.Common.Dtos;
using Connectly.Application.Posts.Dtos;
using MediatR;

namespace Connectly.Application.Posts.Commands.CreatePost;

/// <param name="GroupId">When set, the post is published inside that group (author must be an active member).</param>
public record CreatePostCommand(string? Content, FileUpload? Image, Guid? GroupId = null) : IRequest<PostDto>;
