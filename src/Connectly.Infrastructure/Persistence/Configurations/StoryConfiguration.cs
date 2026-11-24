using Connectly.Domain.Entities.Stories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connectly.Infrastructure.Persistence.Configurations;

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
