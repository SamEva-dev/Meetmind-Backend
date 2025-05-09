using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Models;
using Meetmind.Infrastructure.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MeetingTests.Worker;

public class MeetingSchedulerWorkerTests
{
    [Fact]
    public async Task Should_Notify_When_Meeting_Is_In_5_Min()
    {
        // Arrange
        var calendar = Substitute.For<ICalendarService>();
        var notifier = Substitute.For<INotificationService>();
        var now = DateTime.UtcNow;

        calendar.GetTodayMeetingsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new List<UpcomingMeeting>
            {
                new() { Id = Guid.NewGuid(), Title = "Sprint", StartUtc = now.AddMinutes(5), EndUtc = now.AddMinutes(35), Source = "fake" }
            });

        var services = new ServiceCollection();
        services.AddSingleton(calendar);
        services.AddSingleton(notifier);
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);

        var factory = Substitute.For<IServiceScopeFactory>();
        factory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<MeetingSchedulerWorker>>();
        var worker = new MeetingSchedulerWorker(factory, logger);

        // Act
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await worker.StartAsync(cts.Token);

        // Assert
        await notifier.Received().NotifyUpcomingAsync(Arg.Any<UpcomingMeeting>(), 5, Arg.Any<CancellationToken>());
    }
}