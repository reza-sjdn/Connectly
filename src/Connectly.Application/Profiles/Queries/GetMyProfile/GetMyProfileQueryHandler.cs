using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Profiles.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Profiles.Queries.GetMyProfile;

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
            ?? throw new NotFoundException(nameof(Domain.Entities.UserProfiles.UserProfile), userId);
    }
}
