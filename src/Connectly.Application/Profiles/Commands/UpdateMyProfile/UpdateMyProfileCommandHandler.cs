using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Profiles.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Profiles.Commands.UpdateMyProfile;

public class UpdateMyProfileCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpdateMyProfileCommand, ProfileDto>
{
    public async Task<ProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new AppUnauthorizedException();
        var p = await db.UserProfiles.FirstOrDefaultAsync(x => x.Id == userId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.UserProfiles.UserProfile), userId);

        p.FirstName = request.FirstName.Trim();
        p.LastName = request.LastName.Trim();
        p.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        p.DateOfBirth = request.DateOfBirth;
        await db.SaveChangesAsync(ct);

        return new ProfileDto(p.Id, p.FirstName, p.LastName, p.Bio, p.AvatarUrl, p.CoverUrl, p.DateOfBirth, p.CreatedAtUtc);
    }
}
