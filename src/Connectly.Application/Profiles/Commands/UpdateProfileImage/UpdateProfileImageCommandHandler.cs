using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Profiles.Enums;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Profiles.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IFileStorage storage)
    : IRequestHandler<UpdateProfileImageCommand, string>
{
    public async Task<string> Handle(UpdateProfileImageCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new AppUnauthorizedException();
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId, ct)
                      ?? throw new NotFoundException(nameof(Domain.Entities.UserProfiles.UserProfile), userId);

        if (!await ImageFile.HasValidSignatureAsync(request.File.Content, ct))
            throw new ValidationException([new ValidationFailure("File", "The file is not a valid JPEG, PNG or WebP image.")]);

        var isAvatar = request.Kind == ProfileImageKind.Avatar;
        var safeName = "image" + ImageFile.ExtensionFor(request.File.ContentType);
        var url = await storage.SaveAsync(request.File.Content, safeName, isAvatar ? "avatars" : "covers", ct);

        var old = isAvatar ? profile.AvatarUrl : profile.CoverUrl;
        if (isAvatar) profile.AvatarUrl = url; else profile.CoverUrl = url;
        await db.SaveChangesAsync(ct);

        if (old is not null) await storage.DeleteAsync(old, ct);   // after the DB commit
        return url;
    }
}
