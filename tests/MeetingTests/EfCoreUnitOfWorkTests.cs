using FluentAssertions;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Meetmind.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeetingTests;

public class EfCoreUnitOfWorkTests
{
    private readonly DbContextOptions<MeetMindDbContext> _options;

    public EfCoreUnitOfWorkTests()
    {
        _options = new DbContextOptionsBuilder<MeetMindDbContext>()
            .UseInMemoryDatabase("uow-test") // Ensure the InMemory provider is referenced
            .Options;
    }

    [Fact]
    public async Task Should_Commit_Changes()
    {
        var context = new MeetMindDbContext(_options);
        var uow = new EfCoreUnitOfWork(context);

        context.Meetings.Add(new Meeting("Test UoW", DateTime.UtcNow));
        await uow.SaveChangesAsync(default);

        var saved = await context.Meetings.CountAsync();
        saved.Should().Be(1);
    }
}