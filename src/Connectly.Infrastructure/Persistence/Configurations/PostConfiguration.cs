using Connectly.Domain.Entities.Posts;
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
