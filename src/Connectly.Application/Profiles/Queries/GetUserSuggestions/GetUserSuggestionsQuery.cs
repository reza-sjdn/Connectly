using Connectly.Application.Common.Dtos;
using MediatR;

namespace Connectly.Application.Profiles.Queries.GetUserSuggestions;

/// <summary>People the current user does not follow yet (newest first). Real search arrives in a later slice.</summary>
public record GetUserSuggestionsQuery(int Take = 20) : IRequest<IReadOnlyList<UserSummaryDto>>;
