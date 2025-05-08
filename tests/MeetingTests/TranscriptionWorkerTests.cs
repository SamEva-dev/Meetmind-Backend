using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Db;
using Meetmind.Infrastructure.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeetingTests;

public class TranscriptionWorkerTests
{
    private readonly DbContextOptions<MeetMindDbContext> _options;

    public TranscriptionWorkerTests()
    {
        _options = new DbContextOptionsBuilder<MeetMindDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Should_Process_Queued_Meeting_And_Complete_Transcription()
    {
        // Arrange
        var db = new MeetMindDbContext(_options);
        var meeting = new Meeting("To transcribe", DateTime.UtcNow);
        meeting.QueueTranscription();
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var whisper = Substitute.For<IWhisperService>();
        whisper.TranscribeAsync(meeting.Id, Arg.Any<CancellationToken>())
            .Returns("Data/Transcript/mock.txt");

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton(whisper);
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<TranscriptionWorker>>();
        var worker = new TranscriptionWorker(scopeFactory, logger);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act (run loop once)
        await worker.StartAsync(cts.Token);

        // Assert
        var updated = await db.Meetings.FirstOrDefaultAsync();
        updated!.TranscriptState.Should().Be(TranscriptState.Completed);
        updated.TranscriptPath.Should().Be("Data/Transcript/mock.txt");
    }

    [Fact]
    public async Task Should_Mark_Failed_If_Exception_Occurs()
    {
        // Arrange
        var db = new MeetMindDbContext(_options);
        var meeting = new Meeting("Fail me", DateTime.UtcNow);
        meeting.QueueTranscription();
        db.Meetings.Add(meeting);
        await db.SaveChangesAsync();

        var whisper = Substitute.For<IWhisperService>();
        whisper.TranscribeAsync(meeting.Id, Arg.Any<CancellationToken>())
            .Throws(new Exception("boom"));

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton(whisper);
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<TranscriptionWorker>>();
        var worker = new TranscriptionWorker(scopeFactory, logger);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        var updated = await db.Meetings.FirstOrDefaultAsync();
        updated!.TranscriptState.Should().Be(TranscriptState.Failed);
    }
}