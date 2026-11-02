using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Application.Common.Models;
using Connectly.Application.Groups;
using Connectly.Application.Groups.Models;
using Connectly.Application.Posts;
using Connectly.Application.Posts.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Application.Search;

public record SearchResultDto(
    IReadOnlyList<UserSummaryDto> People, IReadOnlyList<GroupSummaryDto> Groups, IReadOnlyList<PostDto> Posts);

public record SearchQuery(string Term) : IRequest<SearchResultDto>;

public class SearchQueryValidator : AbstractValidator<SearchQuery>
{
    public SearchQueryValidator()
        => RuleFor(x => x.Term).NotEmpty().Must(t => t.Trim().Length is >= 2 and <= 100)
            .WithMessage("Search for 2 to 100 characters.");
}

/// <summary>
/// Plain LIKE search. Every word must match (AND). Fine for a few hundred thousand rows;
/// switch to SQL Server full-text search or a search engine when this becomes slow.
/// </summary>
public class SearchQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<SearchQuery, SearchResultDto>
{
    private const string Esc = "\\";

    public async Task<SearchResultDto> Handle(SearchQuery request, CancellationToken ct)
    {
        var me = currentUser.UserId ?? throw new AppUnauthorizedException();

        var patterns = request.Term.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(4)
            .Select(t => "%" + Escape(t) + "%")
            .ToList();

        // People: each word matches first OR last name
        var people = db.UserProfiles.AsNoTracking().Where(p => p.Id != me);
        foreach (var pattern in patterns)
        {
            var pt = pattern;
            people = people.Where(p => EF.Functions.Like(p.FirstName, pt, Esc) || EF.Functions.Like(p.LastName, pt, Esc));
        }
        var peopleResult = await people
            .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
            .Take(10)
            .Select(p => new UserSummaryDto(p.Id, p.FirstName, p.LastName, p.AvatarUrl,
                db.Follows.Any(f => f.FollowerId == me && f.FolloweeId == p.Id)))
            .ToListAsync(ct);

        // Groups: every word in the name
        var groups = db.Groups.AsNoTracking();
        foreach (var pattern in patterns)
        {
            var pt = pattern;
            groups = groups.Where(g => EF.Functions.Like(g.Name, pt, Esc));
        }
        var groupsResult = await groups.OrderBy(g => g.Name).Take(10).ToSummary(db, me).ToListAsync(ct);

        // Posts: every word in the text; private-group posts only for members (VisibleTo)
        var posts = db.Posts.AsNoTracking().VisibleTo(db, me).Where(p => p.Content != null);
        foreach (var pattern in patterns)
        {
            var pt = pattern;
            posts = posts.Where(p => EF.Functions.Like(p.Content!, pt, Esc));
        }
        var postsResult = await posts.OrderByDescending(p => p.CreatedAtUtc).Take(10).ToDto(db, me).ToListAsync(ct);

        return new SearchResultDto(peopleResult, groupsResult, postsResult);
    }

    // Users must not be able to inject wildcards: escape \ % _ [
    private static string Escape(string s)
        => s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
}
