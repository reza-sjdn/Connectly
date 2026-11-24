using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Stories.Dtos;
using Connectly.Domain.Entities.Stories;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Connectly.Application.Stories.Commands.CreateStory;

public class CreateStoryCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IFileStorage storage, TimeProvider clock)
    : IRequestHandler<CreateStoryCommand, StoryDto>
{
    public const string DefaultBackground = "#1877f2";

    public async Task<StoryDto> Handle(CreateStoryCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var now = clock.GetUtcNow().UtcDateTime;

        var story = new Story { AuthorId = me, CreatedAtUtc = now, ExpiresAtUtc = now.AddHours(24) };

        if (request.Image is not null)
        {
            if (!await ImageFile.HasValidSignatureAsync(request.Image.Content, ct))
                throw new ValidationException([new ValidationFailure("Image", "The file is not a valid JPEG, PNG or WebP image.")]);

            story.ImageUrl = await storage.SaveAsync(
                request.Image.Content, "image" + ImageFile.ExtensionFor(request.Image.ContentType), "stories", ct);
        }
        else
        {
            story.Text = request.Text!.Trim();
            story.BackgroundColor = request.BackgroundColor ?? DefaultBackground;
        }

        db.Stories.Add(story);
        try { await db.SaveChangesAsync(ct); }
        catch
        {
            if (story.ImageUrl is not null) await storage.DeleteAsync(story.ImageUrl, CancellationToken.None);
            throw;
        }

        return new StoryDto(story.Id, story.ImageUrl, story.Text, story.BackgroundColor,
            story.CreatedAtUtc, story.ExpiresAtUtc, true, 0);
    }
}
