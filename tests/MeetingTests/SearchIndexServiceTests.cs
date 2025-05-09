using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Infrastructure.Search;

namespace MeetingTests;

public class SearchIndexServiceTests
{
    [Fact]
    public async Task Should_Index_And_Find_Keyword()
    {
        // Arrange
        var id = Guid.NewGuid();
        var folder = Path.Combine("Data", "Transcript");
        Directory.CreateDirectory(folder);

        var jsonPath = Path.Combine(folder, $"{id}.analysis.json");

        await File.WriteAllTextAsync(jsonPath, $$"""
        {
            "meetingId": "{{id}}",
            "keywords": ["refactor", "tailwindcss", "auth"],
            "participants": [
              { "id": "alice", "label": "Alice", "speakingTurns": 2 },
              { "id": "bob", "label": "Bob", "speakingTurns": 1 }
            ],
            "timeline": [
              { "time": "00:02:12", "speaker": "Alice", "text": "Let’s refactor the login page." },
              { "time": "00:05:10", "speaker": "Bob", "text": "I love TailwindCSS." }
            ]
        }
        """);

        // Act
        var service = new SearchIndexService();
        var results = await service.SearchAsync("tailwind");

        // Assert
        results.Should().NotBeNull();
        results.Should().Contain(e => e.Snippet.Contains("Tailwind", StringComparison.OrdinalIgnoreCase));
        results.Select(r => r.MatchType).Distinct().Should().Contain(new[] { "keyword", "timeline" });
    }

    [Fact]
    public async Task Should_Handle_Empty_Result()
    {
        var service = new SearchIndexService();
        var results = await service.SearchAsync("nonexistentterm");
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Filter_By_Date()
    {
        var today = DateTime.UtcNow;
        var yesterday = today.AddDays(-1);
        var _service = new SearchIndexService();
        var resToday = await _service.SearchAsync("tailwind", from: today);
        resToday.Should().OnlyContain(r => r.DateUtc.Date >= today.Date);

        var resPast = await _service.SearchAsync("tailwind", to: yesterday);
        resPast.Should().OnlyContain(r => r.DateUtc.Date <= yesterday.Date);
    }

}