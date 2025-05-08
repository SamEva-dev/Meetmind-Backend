using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MeetingTests.Integration.Api;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingTests.Hubs;

public class SignalRTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory _factory;

    public SignalRTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Receive_StateChanged_Event_When_Meeting_Started()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();
        var meeting = new Meeting("SignalR test", DateTime.UtcNow);
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var received = new List<(Guid Id, string State)>();

        var hub = new HubConnectionBuilder()
            .WithUrl($"{_client.BaseAddress}hubs/notify", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>("fake-token");
            })
            .Build();

        hub.On<object>("MeetingStateChanged", msg =>
        {
            var json = System.Text.Json.JsonSerializer.Serialize(msg);
            var dyn = System.Text.Json.JsonSerializer.Deserialize<MeetingEvent>(json);
            if (dyn != null)
                received.Add((dyn.MeetingId, dyn.NewState));
        });

        await hub.StartAsync();

        // Act
        var res = await _client.PostAsync($"/v1/meetings/{meeting.Id}/recording/start", null);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Wait for event propagation
        await Task.Delay(300);

        // Assert
        received.Should().ContainSingle(x => x.Id == meeting.Id && x.State == "Recording");

        await hub.DisposeAsync();
    }

    private record MeetingEvent(Guid MeetingId, string NewState);
}