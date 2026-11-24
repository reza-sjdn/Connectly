using Connectly.Domain.Entities.Stories;
using Connectly.Domain.Entities.UserProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connectly.Infrastructure.Persistence.Configurations;

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
