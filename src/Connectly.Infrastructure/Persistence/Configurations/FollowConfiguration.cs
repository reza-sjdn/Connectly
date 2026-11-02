using Connectly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connectly.Infrastructure.Persistence.Configurations;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> b)
    {
        b.ToTable("Follows");
        b.HasKey(x => new { x.FollowerId, x.FolloweeId });   // also prevents duplicate follows
        b.HasIndex(x => x.FolloweeId);                        // "who follows X" lookups

        // Restrict (not Cascade): SQL Server forbids multiple cascade paths to the same table.
        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.FollowerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<UserProfile>().WithMany().HasForeignKey(x => x.FolloweeId).OnDelete(DeleteBehavior.Restrict);
    }
}
