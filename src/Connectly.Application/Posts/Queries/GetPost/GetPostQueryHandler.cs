using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Posts.Dtos;
using Connectly.Domain.Entities.Posts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Posts.Queries.GetPost;

public class GetPostQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetPostQuery, PostDto>
{
    public async Task<PostDto> Handle(GetPostQuery request, CancellationToken ct)
        => await db.Posts.AsNoTracking().Where(p => p.Id == request.PostId)
               .VisibleTo(db, currentUser.UserId).ToDto(db, currentUser.UserId).FirstOrDefaultAsync(ct)
           ?? throw new NotFoundException(nameof(Post), request.PostId);
}
