using Connectly.Domain.Entities.UserProfiles;
using Connectly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connectly.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> b)
    {
        b.ToTable("UserProfiles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();   // same id as the Identity user
        b.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        b.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Bio).HasMaxLength(500);
        b.Property(x => x.AvatarUrl).HasMaxLength(512);
        b.Property(x => x.CoverUrl).HasMaxLength(512);

        b.HasOne<ApplicationUser>().WithOne().HasForeignKey<UserProfile>(x => x.Id)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
