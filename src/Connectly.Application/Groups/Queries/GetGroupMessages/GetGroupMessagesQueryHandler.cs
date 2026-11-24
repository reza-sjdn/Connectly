using Connectly.Application.Common.Dtos;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Helpers;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Groups.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Groups.Queries.GetGroupMessages;

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
