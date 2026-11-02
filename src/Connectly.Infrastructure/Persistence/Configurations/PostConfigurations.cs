using Connectly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connectly.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> b)
    {
        b.ToTable("Posts");
        b.Property(x => x.Content).HasMaxLength(5000);
        b.Property(x => x.ImageUrl).HasMaxLength(512);

        b.HasIndex(x => new { x.AuthorId, x.CreatedAtUtc });   // timeline + feed lookups
        b.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.GroupId, x.CreatedAtUtc });   // group timelines
        // Restrict: groups are not deleted yet; when they are, posts must be handled explicitly.
        b.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> b)
    {
        b.ToTable("Comments");
        b.Property(x => x.Content).IsRequired().HasMaxLength(2000);

        b.HasIndex(x => new { x.PostId, x.CreatedAtUtc });
        b.HasOne(x => x.Post).WithMany().HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Cascade);
        // Restrict: avoids multiple cascade paths (UserProfile -> Comment and UserProfile -> Post -> Comment).
        b.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> b)
    {
        b.ToTable("PostLikes");
        b.HasKey(x => new { x.PostId, x.UserId });

        b.HasOne<Post>().WithMany().HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
