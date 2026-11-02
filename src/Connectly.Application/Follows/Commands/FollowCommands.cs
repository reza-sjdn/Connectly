using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Follows.Commands;

public record FollowUserCommand(Guid UserId) : IRequest;

public class FollowUserCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IUserNotificationPublisher notifications)
    : IRequestHandler<FollowUserCommand>
{
    public async Task Handle(FollowUserCommand request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        if (me == request.UserId)
            throw new ValidationException([new ValidationFailure(nameof(request.UserId), "You cannot follow yourself.")]);

        if (!await db.UserProfiles.AnyAsync(p => p.Id == request.UserId, ct))
            throw new NotFoundException(nameof(UserProfile), request.UserId);

        // Idempotent: following twice is a no-op.
        if (await db.Follows.AnyAsync(f => f.FollowerId == me && f.FolloweeId == request.UserId, ct)) return;

        db.Follows.Add(new Follow { FollowerId = me, FolloweeId = request.UserId });
        await db.SaveChangesAsync(ct);

        await notifications.PublishAsync(request.UserId, me, NotificationType.NewFollower, null, ct);
    }
}

public record UnfollowUserCommand(Guid UserId) : IRequest;

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
