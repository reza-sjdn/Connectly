using Connectly.Domain.Entities;
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

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> b)
    {
        b.ToTable("Conversations");
        b.HasIndex(x => new { x.UserAId, x.UserBId }).IsUnique();   // one conversation per pair
        b.HasIndex(x => x.UserBId);                                  // lookups where I am the "B" side
        b.HasIndex(x => x.LastMessageAtUtc);

        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.UserAId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.UserBId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.ToTable("Messages");
        b.Property(x => x.Content).IsRequired().HasMaxLength(2000);
        b.HasIndex(x => new { x.ConversationId, x.CreatedAtUtc });

        b.HasOne(x => x.Conversation).WithMany().HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
    }
}
