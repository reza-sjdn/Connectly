using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Follows.Commands.UnfollowUser;

public class UnfollowUserCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UnfollowUserCommand>
{
    public async Task Handle(UnfollowUserCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await db.Follows
            .Where(f => f.FollowerId == me && f.FolloweeId == request.UserId)
            .ExecuteDeleteAsync(ct);
    }
}
