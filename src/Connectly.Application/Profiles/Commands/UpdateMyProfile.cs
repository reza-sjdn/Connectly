using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Profiles.Queries;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Profiles.Commands;

public record UpdateMyProfileCommand(string FirstName, string LastName, string? Bio, DateOnly? DateOfBirth)
    : IRequest<ProfileDto>;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Bio).MaximumLength(500);
        RuleFor(x => x.DateOfBirth)
            .Must(d => d is null || d <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-13))
            .WithMessage("You must be at least 13 years old.");
    }
}

public class UpdateMyProfileCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpdateMyProfileCommand, ProfileDto>
{
    public async Task<ProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new AppUnauthorizedException();
        var p = await db.UserProfiles.FirstOrDefaultAsync(x => x.Id == userId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.UserProfile), userId);

        p.FirstName = request.FirstName.Trim();
        p.LastName = request.LastName.Trim();
        p.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        p.DateOfBirth = request.DateOfBirth;
        await db.SaveChangesAsync(ct);

        return new ProfileDto(p.Id, p.FirstName, p.LastName, p.Bio, p.AvatarUrl, p.CoverUrl, p.DateOfBirth, p.CreatedAtUtc);
    }
}
