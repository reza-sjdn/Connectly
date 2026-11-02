using Connectly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connectly.Infrastructure.Persistence.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> b)
    {
        b.ToTable("Groups");
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasIndex(x => x.Name);
        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> b)
    {
        b.ToTable("GroupMembers");
        b.HasKey(x => new { x.GroupId, x.UserId });            // one membership row per user and group
        b.HasIndex(x => new { x.UserId, x.Status });            // "my groups"
        b.HasOne<Group>().WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GroupMessageConfiguration : IEntityTypeConfiguration<GroupMessage>
{
    public void Configure(EntityTypeBuilder<GroupMessage> b)
    {
        b.ToTable("GroupMessages");
        b.Property(x => x.Content).IsRequired().HasMaxLength(2000);
        b.HasIndex(x => new { x.GroupId, x.CreatedAtUtc });
        b.HasOne<Group>().WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> b)
    {
        b.ToTable("Stories");
        b.Property(x => x.ImageUrl).HasMaxLength(512);
        b.Property(x => x.Text).HasMaxLength(280);
        b.Property(x => x.BackgroundColor).HasMaxLength(7);
        b.HasIndex(x => new { x.AuthorId, x.ExpiresAtUtc });
        b.HasIndex(x => x.ExpiresAtUtc);                        // cleanup job
        b.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StoryViewConfiguration : IEntityTypeConfiguration<StoryView>
{
    public void Configure(EntityTypeBuilder<StoryView> b)
    {
        b.ToTable("StoryViews");
        b.HasKey(x => new { x.StoryId, x.ViewerId });
        b.HasOne<Story>().WithMany().HasForeignKey(x => x.StoryId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.ViewerId).OnDelete(DeleteBehavior.Restrict);
    }
}
