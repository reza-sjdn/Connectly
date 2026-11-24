using Connectly.Domain.Entities.Chats;
using Connectly.Domain.Entities.UserProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Connectly.Infrastructure.Persistence.Configurations;

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
