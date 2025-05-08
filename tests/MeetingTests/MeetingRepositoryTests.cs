using FluentAssertions;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Meetmind.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeetingTests;

public class MeetingRepositoryTests
{
    private readonly DbContextOptions<MeetMindDbContext> _options;

    public MeetingRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<MeetMindDbContext>()
            .UseInMemoryDatabase("meetings-test")
            .Options;
    }

    [Fact]
    public async Task Should_Save_And_Load_Meeting()
    {
        var meeting = new Meeting("Sync", DateTime.UtcNow);

        using (var context = new MeetMindDbContext(_options))
        {
            context.Meetings.Add(meeting);
            await context.SaveChangesAsync();
        }

        using (var context = new MeetMindDbContext(_options))
        {
            var repo = new MeetingRepository(context);
            var found = await repo.GetByIdAsync(meeting.Id, default);

            found.Should().NotBeNull();
            found!.Title.Should().Be("Sync");
        }
    }
}