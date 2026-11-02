using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using Connectly.Application.Posts;
using Connectly.Application.Posts.Models;
using Connectly.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Comments.Queries;

/// <summary>Oldest first, so new comments append at the end and page offsets stay stable.</summary>
public record GetCommentsQuery(Guid PostId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<CommentDto>>;

public class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
{
    public GetCommentsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

public class GetCommentsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetCommentsQuery, PagedResult<CommentDto>>
{
    public async Task<PagedResult<CommentDto>> Handle(GetCommentsQuery request, CancellationToken ct)
    {
        if (!await db.Posts.VisibleTo(db, currentUser.UserId).AnyAsync(p => p.Id == request.PostId, ct))
            throw new NotFoundException(nameof(Post), request.PostId);

        var query = db.Comments.AsNoTracking().Where(c => c.PostId == request.PostId);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.CreatedAtUtc).ThenBy(c => c.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToDto(currentUser.UserId)
            .ToListAsync(ct);

        return new PagedResult<CommentDto>(items, request.Page, request.PageSize, total);
    }
}
