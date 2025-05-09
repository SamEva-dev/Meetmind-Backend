using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Search;

namespace MeetingTests.Integration.Api;

public class SearchControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly string _dataPath = Path.Combine("Data", "Transcript");

    public SearchControllerTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient();
        Directory.CreateDirectory(_dataPath);
    }

    [Fact]
    public async Task Should_Return_Results_For_Indexed_Term()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var jsonPath = Path.Combine(_dataPath, $"{meetingId}.analysis.json");

        await File.WriteAllTextAsync(jsonPath, $$"""
        {
            "meetingId": "{{meetingId}}",
            "keywords": ["tailwindcss", "demo"],
            "participants": [
              { "id": "alice", "label": "Alice", "speakingTurns": 3 }
            ],
            "timeline": [
              { "time": "00:01:00", "speaker": "Alice", "text": "Let's demo TailwindCSS." }
            ]
        }
        
        """);

        // Recharger le service (forçage manuel si besoin)
        var service = new SearchIndexService();

        // Act
        var res = await _client.GetFromJsonAsync<List<JsonElement>>("/v1/search?q=tailwind");

        // Assert
        res.Should().NotBeNull();
        res!.Count.Should().BeGreaterThan(0);
        res!.Any(r => r.GetProperty("snippet").ToString().Contains("Tailwind", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task Should_Return_Empty_List_If_Not_Found()
    {
        var res = await _client.GetFromJsonAsync<List<JsonElement>>("/v1/search?q=xyznotfound");
        res.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return_400_If_Missing_Query()
    {
        var res = await _client.GetAsync("/v1/search");
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}