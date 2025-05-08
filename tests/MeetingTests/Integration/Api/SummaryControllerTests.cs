using System.Net;
using FluentAssertions;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingTests.Integration.Api;

public class SummaryControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory _factory;

    public SummaryControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Accept_Summary_Trigger()
    {
        var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<MeetMindDbContext>();
        var meeting = new Meeting("Résumé manuel", DateTime.UtcNow);
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var res = await _client.PostAsync($"/v1/meetings/{meeting.Id}/summary", null);
        res.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task Should_Return_Summary_When_Ready()
    {
        var id = Guid.NewGuid();
        var path = $"Data/Summary/{id}.md";
        Directory.CreateDirectory("Data/Summary");
        await File.WriteAllTextAsync(path, "## Résumé Markdown");

        var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<MeetMindDbContext>();
        var meeting = new Meeting("Résumé dispo", DateTime.UtcNow);
        meeting.QueueSummary();
        meeting.MarkSummaryProcessing();
        meeting.MarkSummaryCompleted(path);
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var res = await _client.GetAsync($"/v1/meetings/{id}/summary");
        var body = await res.Content.ReadAsStringAsync();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("text/markdown");
        body.Should().Contain("##");
    }

    [Fact]
    public async Task Should_Return_425_If_Not_Ready()
    {
        var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<MeetMindDbContext>();
        var meeting = new Meeting("Not ready", DateTime.UtcNow);
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var res = await _client.GetAsync($"/v1/meetings/{meeting.Id}/summary");
        res.StatusCode.Should().Be((HttpStatusCode)425);
    }
}