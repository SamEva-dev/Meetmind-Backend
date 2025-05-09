using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Application.Search;

namespace Meetmind.Infrastructure.Search;

public class SearchIndexService : ISearchService
{
    private readonly List<SearchEntry> _index = [];

    public SearchIndexService()
    {
        var folder = Path.Combine("Data", "Transcript");
        if (!Directory.Exists(folder)) return;

        foreach (var file in Directory.GetFiles(folder, "*.analysis.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var root = JsonDocument.Parse(json).RootElement;
                var meetingId = root.GetProperty("meetingId").GetGuid();

                var dateUtc = root.TryGetProperty("dateUtc", out var dateJson)
                        ? dateJson.GetDateTime()
                        : File.GetCreationTimeUtc(file);


                // Index keywords
                foreach (var keyword in root.GetProperty("keywords").EnumerateArray())
                {
                    _index.Add(new SearchEntry
                    {
                        MeetingId = meetingId,
                        MatchType = "keyword",
                        Snippet = keyword.GetString() ?? "",
                        Score = 1.5,
                        DateUtc = dateUtc
                    });
                }

                // Index participants
                foreach (var participant in root.GetProperty("participants").EnumerateArray())
                {
                    var label = participant.GetProperty("label").GetString();
                    if (!string.IsNullOrWhiteSpace(label))
                        _index.Add(new SearchEntry
                        {
                            MeetingId = meetingId,
                            MatchType = "participant",
                            Snippet = label,
                            Score = 1.0,
                            DateUtc = dateUtc
                        });
                }

                // Index timeline
                foreach (var entry in root.GetProperty("timeline").EnumerateArray())
                {
                    _index.Add(new SearchEntry
                    {
                        MeetingId = meetingId,
                        MatchType = "timeline",
                        Snippet = entry.GetProperty("text").GetString() ?? "",
                        Score = 0.9,
                        DateUtc = dateUtc
                    });
                }
            }
            catch
            {
                // Skip invalid file
            }
        }
    }

    public Task<List<SearchEntry>> SearchAsync(string query, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var q = query.ToLowerInvariant();
        var results = _index
       .Where(e => e.Snippet.ToLowerInvariant().Contains(q))
       .Where(e => from == null || e.DateUtc >= from.Value.Date)
       .Where(e => to == null || e.DateUtc <= to.Value.Date)
       .OrderByDescending(e => e.Score)
       .Take(50)
       .ToList();

        return Task.FromResult(results);
    }
}