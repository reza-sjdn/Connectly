using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Profiles.Queries;

/// <summary>The signed-in user's own profile (includes private fields such as date of birth).</summary>
public record ProfileDto(
    Guid Id, string FirstName, string LastName, string? Bio,
    string? AvatarUrl, string? CoverUrl, DateOnly? DateOfBirth, DateTime CreatedAtUtc);

public record GetMyProfileQuery : IRequest<ProfileDto>;

public class GetMyProfileQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new AppUnauthorizedException();

        return await db.UserProfiles
            .AsNoTracking()
            .Where(p => p.Id == userId)
            .Select(p => new ProfileDto(p.Id, p.FirstName, p.LastName, p.Bio, p.AvatarUrl, p.CoverUrl, p.DateOfBirth, p.CreatedAtUtc))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.UserProfile), userId);
    }
}
