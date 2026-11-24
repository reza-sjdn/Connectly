using Connectly.Application.Search.Dtos;
using MediatR;

namespace Connectly.Application.Search.Search;

public record SearchQuery(string Term) : IRequest<SearchResultDto>;
