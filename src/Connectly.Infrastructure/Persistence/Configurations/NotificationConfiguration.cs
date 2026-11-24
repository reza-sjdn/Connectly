using Connectly.Domain.Entities.Notifications;
using Connectly.Domain.Entities.Posts;
using Connectly.Domain.Entities.UserProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connectly.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasIndex(x => new { x.RecipientId, x.CreatedAtUtc });
        b.HasIndex(x => new { x.RecipientId, x.IsRead });

        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.RecipientId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        // Restrict: Group -> Post -> Notification already cascades; a second path would be rejected by SQL Server.
        b.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
        // Deleting a post removes the notifications that point at it.
        b.HasOne<Post>().WithMany().HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Cascade);
    }
}
