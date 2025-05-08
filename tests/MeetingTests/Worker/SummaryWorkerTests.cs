using FluentAssertions;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Db;
using Meetmind.Infrastructure.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MeetingTests.Worker;

public class SummaryWorkerTests
{
    private readonly DbContextOptions<MeetMindDbContext> _options;

    public SummaryWorkerTests()
    {
        _options = new DbContextOptionsBuilder<MeetMindDbContext>()
            .UseInMemoryDatabase("summary-test-" + Guid.NewGuid())
            .Options;
    }

    [Fact]
    public async Task Should_Complete_Summary_When_Success()
    {
        // Arrange
        var db = new MeetMindDbContext(_options);
        var meeting = new Meeting("To summarize", DateTime.UtcNow);
        meeting.QueueSummary();
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var summaryService = Substitute.For<ISummaryService>();
        summaryService.GenerateSummaryAsync(meeting.Id, Arg.Any<CancellationToken>())
            .Returns($"Data/Summary/{meeting.Id}.md");

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton(summaryService);
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<SummaryWorker>>();
        var worker = new SummaryWorker(scopeFactory, logger);

        // Run one cycle
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await worker.StartAsync(cts.Token);

        var updated = await db.Meetings.FindAsync(meeting.Id);
        updated!.SummaryState.Should().Be(SummaryState.Completed);
        updated.SummaryPath.Should().Contain(meeting.Id.ToString());
    }

    [Fact]
    public async Task Should_Mark_Failed_If_Service_Throws()
    {
        // Arrange
        var db = new MeetMindDbContext(_options);
        var meeting = new Meeting("To fail", DateTime.UtcNow);
        meeting.QueueSummary();
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var summaryService = Substitute.For<ISummaryService>();
        summaryService.GenerateSummaryAsync(meeting.Id, Arg.Any<CancellationToken>())
            .Throws(new Exception("Mock error"));

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton(summaryService);
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<SummaryWorker>>();
        var worker = new SummaryWorker(scopeFactory, logger);

        // Act
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await worker.StartAsync(cts.Token);

        var updated = await db.Meetings.FindAsync(meeting.Id);
        updated!.SummaryState.Should().Be(SummaryState.Failed);
    }
}