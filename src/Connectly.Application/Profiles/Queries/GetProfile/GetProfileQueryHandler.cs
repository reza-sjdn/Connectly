using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Profiles.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Profiles.Queries.GetProfile;

public class GetProfileQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetProfileQuery, PublicProfileDto>
{
    public async Task<PublicProfileDto> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId;

        return await db.UserProfiles
            .AsNoTracking()
            .Where(p => p.Id == request.UserId)
            .Select(p => new PublicProfileDto(
                p.Id, p.FirstName, p.LastName, p.Bio, p.AvatarUrl, p.CoverUrl, p.CreatedAtUtc,
                db.Follows.Count(f => f.FolloweeId == p.Id),
                db.Follows.Count(f => f.FollowerId == p.Id),
                p.Id == me,
                db.Follows.Any(f => f.FollowerId == me && f.FolloweeId == p.Id)))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.UserProfiles.UserProfile), request.UserId);
    }
}
