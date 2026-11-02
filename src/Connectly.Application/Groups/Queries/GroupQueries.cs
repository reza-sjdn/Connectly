using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using Connectly.Application.Groups.Models;
using Connectly.Application.Posts;
using Connectly.Application.Posts.Models;
using Connectly.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Queries;

// ---------- list ----------
public record GetGroupsQuery(GroupListScope Scope) : IRequest<IReadOnlyList<GroupSummaryDto>>;

public class GetGroupsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupsQuery, IReadOnlyList<GroupSummaryDto>>
{
    public async Task<IReadOnlyList<GroupSummaryDto>> Handle(GetGroupsQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        var groups = db.Groups.AsNoTracking();

        // "Mine" = groups I am an active member of; "Discover" = everything else (pending requests included).
        groups = request.Scope == GroupListScope.Mine
            ? groups.Where(g => db.GroupMembers.Any(m => m.GroupId == g.Id && m.UserId == me && m.Status == MembershipStatus.Active))
                    .OrderBy(g => g.Name)
            : groups.Where(g => !db.GroupMembers.Any(m => m.GroupId == g.Id && m.UserId == me && m.Status == MembershipStatus.Active))
                    .OrderByDescending(g => g.CreatedAtUtc);

        return await groups.Take(50).ToSummary(db, me).ToListAsync(ct);
    }
}

// ---------- details (name/description/member count are visible even for private groups) ----------
public record GetGroupQuery(Guid GroupId) : IRequest<GroupDetailsDto>;

public class GetGroupQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupQuery, GroupDetailsDto>
{
    public Task<GroupDetailsDto> Handle(GetGroupQuery request, CancellationToken ct)
        => GroupProjection.DetailsAsync(db, request.GroupId, currentUser.UserId, ct);
}

// ---------- members ----------
public record GetGroupMembersQuery(Guid GroupId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<GroupMemberDto>>;

public class GetGroupMembersQueryValidator : AbstractValidator<GetGroupMembersQuery>
{
    public GetGroupMembersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

public class GetGroupMembersQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupMembersQuery, PagedResult<GroupMemberDto>>
{
    public async Task<PagedResult<GroupMemberDto>> Handle(GetGroupMembersQuery request, CancellationToken ct)
    {
        var group = await GroupAccess.GetGroupAsync(db, request.GroupId, ct);
        await GroupAccess.RequireCanViewAsync(db, group, currentUser.UserId, ct);

        var query = db.GroupMembers.AsNoTracking()
            .Where(m => m.GroupId == request.GroupId && m.Status == MembershipStatus.Active);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.Role).ThenBy(m => m.CreatedAtUtc)   // admins first
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(m => new GroupMemberDto(
                new AuthorDto(m.User.Id, m.User.FirstName, m.User.LastName, m.User.AvatarUrl), m.Role, m.CreatedAtUtc))
            .ToListAsync(ct);

        return new PagedResult<GroupMemberDto>(items, request.Page, request.PageSize, total);
    }
}

// ---------- pending join requests (admins only) ----------
public record GetJoinRequestsQuery(Guid GroupId) : IRequest<IReadOnlyList<JoinRequestDto>>;

public class GetJoinRequestsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetJoinRequestsQuery, IReadOnlyList<JoinRequestDto>>
{
    public async Task<IReadOnlyList<JoinRequestDto>> Handle(GetJoinRequestsQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireAdminAsync(db, request.GroupId, me, ct);

        return await db.GroupMembers.AsNoTracking()
            .Where(m => m.GroupId == request.GroupId && m.Status == MembershipStatus.Pending)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new JoinRequestDto(
                new AuthorDto(m.User.Id, m.User.FirstName, m.User.LastName, m.User.AvatarUrl), m.CreatedAtUtc))
            .ToListAsync(ct);
    }
}

// ---------- group posts ----------
public record GetGroupPostsQuery(Guid GroupId, string? Cursor = null, int PageSize = 10) : IRequest<CursorPage<PostDto>>;

public class GetGroupPostsQueryValidator : AbstractValidator<GetGroupPostsQuery>
{
    public GetGroupPostsQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 30);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}

public class GetGroupPostsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupPostsQuery, CursorPage<PostDto>>
{
    public async Task<CursorPage<PostDto>> Handle(GetGroupPostsQuery request, CancellationToken ct)
    {
        var group = await GroupAccess.GetGroupAsync(db, request.GroupId, ct);
        await GroupAccess.RequireCanViewAsync(db, group, currentUser.UserId, ct);

        var posts = db.Posts.AsNoTracking().Where(p => p.GroupId == request.GroupId);
        return await PostProjection.PageAsync(posts, db, currentUser.UserId, request.Cursor, request.PageSize, ct);
    }
}

// ---------- group chat history (members only, newest first) ----------
public record GetGroupMessagesQuery(Guid GroupId, string? Cursor = null, int PageSize = 30) : IRequest<CursorPage<GroupMessageDto>>;

public class GetGroupMessagesQueryValidator : AbstractValidator<GetGroupMessagesQuery>
{
    public GetGroupMessagesQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Cursor).Must(c => c is null || PageCursor.TryDecode(c, out _, out _)).WithMessage("Invalid cursor.");
    }
}

public class GetGroupMessagesQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGroupMessagesQuery, CursorPage<GroupMessageDto>>
{
    public async Task<CursorPage<GroupMessageDto>> Handle(GetGroupMessagesQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();
        await GroupAccess.RequireMemberAsync(db, request.GroupId, me, ct);

        var query = db.GroupMessages.AsNoTracking().Where(m => m.GroupId == request.GroupId);
        if (PageCursor.TryDecode(request.Cursor, out var at, out var id))
            query = query.Where(m => m.CreatedAtUtc < at || (m.CreatedAtUtc == at && m.Id.CompareTo(id) < 0));

        var rows = await query
            .OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.Id)
            .Take(request.PageSize + 1)
            .Select(m => new GroupMessageDto(m.Id, m.GroupId,
                new AuthorDto(m.Sender.Id, m.Sender.FirstName, m.Sender.LastName, m.Sender.AvatarUrl),
                m.Content, m.CreatedAtUtc))
            .ToListAsync(ct);

        var hasMore = rows.Count > request.PageSize;
        var items = hasMore ? rows.Take(request.PageSize).ToList() : rows;
        return new CursorPage<GroupMessageDto>(items, hasMore ? PageCursor.Encode(items[^1].CreatedAtUtc, items[^1].Id) : null);
    }
}
