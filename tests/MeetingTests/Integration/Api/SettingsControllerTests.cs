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
using Microsoft.Extensions.DependencyInjection;

namespace MeetingTests.Integration.Api;

public class SettingsControllerTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _provider;
    private readonly Guid _userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public SettingsControllerTests(CustomWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _provider = factory.Services;
    }

    [Fact]
    public async Task Should_Get_User_Settings()
    {
        var db = _provider.CreateScope().ServiceProvider.GetRequiredService<MeetMindDbContext>();
        db.UserSettings.Add(new UserSettingsEntity { Id = _userId, AutoStartRecord = true });
        await db.SaveChangesAsync();

        var res = await _client.GetAsync("/v1/settings");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        dto.Should().ContainKey("autoStartRecord").WhoseValue.ToString().Should().Be("True");
    }

    [Fact]
    public async Task Should_Update_User_Settings()
    {
        var db = _provider.CreateScope().ServiceProvider.GetRequiredService<MeetMindDbContext>();
        db.UserSettings.Add(new UserSettingsEntity { Id = _userId });
        await db.SaveChangesAsync();

        var payload = new
        {
            autoStartRecord = true,
            autoTranscript = true,
            autoSummarize = false,
            autoTranslate = false,
            notifyBeforeMinutes = new[] { 5, 1 }
        };

        var res = await _client.PutAsJsonAsync("/v1/settings", payload);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var entity = await db.UserSettings.FindAsync(_userId);
        entity!.AutoStartRecord.Should().BeTrue();
        entity.AutoTranscript.Should().BeTrue();
    }
}