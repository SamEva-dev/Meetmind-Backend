using System.Net.Http.Json;
using System.Net;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Meetmind.Domain.Enums;

namespace MeetingTests.Integration.Api;

public class MeetingsControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory _factory;

    public MeetingsControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Start_And_Stop_Meeting()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();

        var meeting = new Meeting("Test REST", DateTime.UtcNow);
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        // Act: Start
        var start = await _client.PostAsync($"/v1/meetings/{meeting.Id}/recording/start", null);
        start.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act: Stop
        var stop = await _client.PostAsync($"/v1/meetings/{meeting.Id}/recording/stop", null);
        stop.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify DB state
        var updated = await db.Meetings.FindAsync(meeting.Id);
        updated.Should().NotBeNull();
        updated!.State.Should().Be(MeetingState.Done);
        updated.EndUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Return_Today_Meetings()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();

        db.Meetings.Add(new Meeting("Today meeting", DateTime.UtcNow));
        await db.SaveChangesAsync();

        var res = await _client.GetFromJsonAsync<List<dynamic>>("/v1/meetings/today");
        res.Should().NotBeNull();
        res!.Count.Should().BeGreaterThan(0);
    }
}