using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using Connectly.Application.Posts.Models;
using Connectly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts;

internal static class PostProjection
{
    public static IQueryable<PostDto> ToDto(this IQueryable<Post> posts, IApplicationDbContext db, Guid? me) =>
        posts.Select(p => new PostDto(
            p.Id,
            new AuthorDto(p.Author.Id, p.Author.FirstName, p.Author.LastName, p.Author.AvatarUrl),
            p.Content, p.ImageUrl, p.CreatedAtUtc,
            db.PostLikes.Count(l => l.PostId == p.Id),
            db.Comments.Count(c => c.PostId == p.Id),
            db.PostLikes.Any(l => l.PostId == p.Id && l.UserId == me),
            p.AuthorId == me,
            p.GroupId,
            p.Group == null ? null : p.Group.Name));

    public static IQueryable<CommentDto> ToDto(this IQueryable<Comment> comments, Guid? me) =>
        comments.Select(c => new CommentDto(
            c.Id, c.PostId,
            new AuthorDto(c.Author.Id, c.Author.FirstName, c.Author.LastName, c.Author.AvatarUrl),
            c.Content, c.CreatedAtUtc,
            c.AuthorId == me,
            c.AuthorId == me || c.Post.AuthorId == me));   // post owners may remove comments on their post

    /// <summary>
    /// A post is visible if it is a normal post, or belongs to a public group, or the viewer is an active member
    /// of its (private) group. Every read or interaction with a single post goes through this filter.
    /// </summary>
    public static IQueryable<Post> VisibleTo(this IQueryable<Post> posts, IApplicationDbContext db, Guid? me) =>
        posts.Where(p => p.GroupId == null
                         || p.Group!.Privacy == GroupPrivacy.Public
                         || db.GroupMembers.Any(m => m.GroupId == p.GroupId && m.UserId == me
                                                     && m.Status == MembershipStatus.Active));

    /// <summary>Newest-first keyset paging over an already filtered post query.</summary>
    public static async Task<CursorPage<PostDto>> PageAsync(
        IQueryable<Post> filtered, IApplicationDbContext db, Guid? me,
        string? cursor, int pageSize, CancellationToken ct)
    {
        if (PageCursor.TryDecode(cursor, out var at, out var id))
            filtered = filtered.Where(p => p.CreatedAtUtc < at || (p.CreatedAtUtc == at && p.Id.CompareTo(id) < 0));

        var rows = await filtered
            .OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id)
            .Take(pageSize + 1)                       // one extra row tells us whether there is a next page
            .ToDto(db, me)
            .ToListAsync(ct);

        var hasMore = rows.Count > pageSize;
        var items = hasMore ? rows.Take(pageSize).ToList() : rows;
        var next = hasMore ? PageCursor.Encode(items[^1].CreatedAtUtc, items[^1].Id) : null;
        return new CursorPage<PostDto>(items, next);
    }
}
