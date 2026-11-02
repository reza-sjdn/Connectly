using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using Connectly.Application.Stories.Models;
using Connectly.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Stories.Commands;

/// <summary>A story is EITHER a photo OR text on a colored background, and disappears after 24 hours.</summary>
public record CreateStoryCommand(string? Text, string? BackgroundColor, FileUpload? Image) : IRequest<StoryDto>;

public class CreateStoryCommandValidator : AbstractValidator<CreateStoryCommand>
{
    public CreateStoryCommandValidator()
    {
        RuleFor(x => x.Text).MaximumLength(280);
        RuleFor(x => x)
            .Must(x => (x.Image is not null) ^ !string.IsNullOrWhiteSpace(x.Text))
            .OverridePropertyName("Text")
            .WithMessage("Share either a photo or some text.");
        RuleFor(x => x.BackgroundColor)
            .Must(c => c is null || System.Text.RegularExpressions.Regex.IsMatch(c, "^#[0-9a-fA-F]{6}$"))
            .WithMessage("Background color must look like #1877f2.");
        When(x => x.Image is not null, () => RuleFor(x => x.Image!).SetValidator(new ImageUploadValidator()));
    }
}

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

public record DeleteStoryCommand(Guid StoryId) : IRequest;

public class DeleteStoryCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IFileStorage storage)
    : IRequestHandler<DeleteStoryCommand>
{
    public async Task Handle(DeleteStoryCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var story = await db.Stories.FirstOrDefaultAsync(s => s.Id == request.StoryId, ct)
                    ?? throw new NotFoundException(nameof(Story), request.StoryId);
        if (story.AuthorId != me) throw new ForbiddenException("You can only delete your own stories.");

        var image = story.ImageUrl;
        db.Stories.Remove(story);                 // views are removed by FK cascade
        await db.SaveChangesAsync(ct);
        if (image is not null) await storage.DeleteAsync(image, ct);
    }
}

/// <summary>Records that I saw someone's story (idempotent; the author's own views are not counted).</summary>
public record MarkStoryViewedCommand(Guid StoryId) : IRequest;

public class MarkStoryViewedCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : IRequestHandler<MarkStoryViewedCommand>
{
    public async Task Handle(MarkStoryViewedCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var now = clock.GetUtcNow().UtcDateTime;

        var authorId = await db.Stories
            .Where(s => s.Id == request.StoryId && s.ExpiresAtUtc > now
                        && (s.AuthorId == me || db.Follows.Any(f => f.FollowerId == me && f.FolloweeId == s.AuthorId)))
            .Select(s => (Guid?)s.AuthorId)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Story), request.StoryId);

        if (authorId == me) return;
        if (await db.StoryViews.AnyAsync(v => v.StoryId == request.StoryId && v.ViewerId == me, ct)) return;

        db.StoryViews.Add(new StoryView { StoryId = request.StoryId, ViewerId = me, ViewedAtUtc = now });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { /* viewed from two tabs at once */ }
    }
}
