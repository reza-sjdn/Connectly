using Connectly.Domain.Entities.Chats;
using Connectly.Domain.Entities.Follows;
using Connectly.Domain.Entities.Groups;
using Connectly.Domain.Entities.Notifications;
using Connectly.Domain.Entities.Posts;
using Connectly.Domain.Entities.Stories;
using Connectly.Domain.Entities.UserProfiles;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Follow> Follows { get; }
    DbSet<Post> Posts { get; }
    DbSet<Comment> Comments { get; }
    DbSet<PostLike> PostLikes { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupMember> GroupMembers { get; }
    DbSet<GroupMessage> GroupMessages { get; }
    DbSet<Story> Stories { get; }
    DbSet<StoryView> StoryViews { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
