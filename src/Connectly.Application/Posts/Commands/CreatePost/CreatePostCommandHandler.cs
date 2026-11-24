using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups;
using Connectly.Application.Posts.Dtos;
using Connectly.Domain.Entities.Posts;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts.Commands.CreatePost;

public class CreatePostCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IFileStorage storage)
    : IRequestHandler<CreatePostCommand, PostDto>
{
    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        if (request.GroupId is { } groupId)
            await GroupAccess.RequireMemberAsync(db, groupId, me, ct);

        string? imageUrl = null;
        if (request.Image is not null)
        {
            if (!await ImageFile.HasValidSignatureAsync(request.Image.Content, ct))
                throw new ValidationException([new ValidationFailure("Image", "The file is not a valid JPEG, PNG or WebP image.")]);

            imageUrl = await storage.SaveAsync(
                request.Image.Content, "image" + ImageFile.ExtensionFor(request.Image.ContentType), "posts", ct);
        }

        var post = new Post
        {
            AuthorId = me,
            GroupId = request.GroupId,
            Content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim(),
            ImageUrl = imageUrl
        };
        db.Posts.Add(post);

        try { await db.SaveChangesAsync(ct); }
        catch
        {
            if (imageUrl is not null) await storage.DeleteAsync(imageUrl, CancellationToken.None);   // no orphan files
            throw;
        }

        return await db.Posts.AsNoTracking().Where(p => p.Id == post.Id).ToDto(db, me).FirstAsync(ct);
    }
}
