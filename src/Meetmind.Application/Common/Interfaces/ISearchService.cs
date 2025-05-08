
using Meetmind.Application.Search;

namespace Meetmind.Application.Common.Interfaces;

public interface ISearchService
{
    Task<List<SearchEntry>> SearchAsync(string query, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
}
