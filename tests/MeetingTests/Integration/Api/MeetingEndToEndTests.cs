using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Meetmind.Domain.Enums;

namespace MeetingTests.Integration.Api;

public class MeetingEndToEndTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory _factory;

    public MeetingEndToEndTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Start_Stop_Notify_And_Query_Meeting()
    {
        var db = _factory.Services.CreateScope()
                 .ServiceProvider.GetRequiredService<MeetMindDbContext>();

        var meeting = new Meeting("Full cycle", DateTime.UtcNow);
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var received = new List<string>();

        var hub = new HubConnectionBuilder()
            .WithUrl($"{_client.BaseAddress}hubs/notify", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>("mock-token");
            })
            .Build();

        hub.On<object>("MeetingStateChanged", msg =>
        {
            var json = System.Text.Json.JsonSerializer.Serialize(msg);
            var dyn = System.Text.Json.JsonSerializer.Deserialize<MeetingEvent>(json);
            if (dyn != null) received.Add(dyn.NewState);
        });

        await hub.StartAsync();

        // 1. Start
        var startRes = await _client.PostAsync($"/v1/meetings/{meeting.Id}/recording/start", null);
        startRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 2. Stop
        var stopRes = await _client.PostAsync($"/v1/meetings/{meeting.Id}/recording/stop", null);
        stopRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 3. Wait for SignalR propagation
        await Task.Delay(300);
        received.Should().Contain("Recording").And.Contain("Done");

        // 4. Check updated state in DB
        var updated = await db.Meetings.FindAsync(meeting.Id);
        updated!.State.Should().Be(MeetingState.Done);
        updated.EndUtc.Should().NotBeNull();

        // 5. Check listing via API
        var list = await _client.GetFromJsonAsync<List<dynamic>>("/v1/meetings/today");
       // list.Should().Contain(x => x.title == "Full cycle");

        await hub.DisposeAsync();
    }

    private record MeetingEvent(Guid MeetingId, string NewState);
}