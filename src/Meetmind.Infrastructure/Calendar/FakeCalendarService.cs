using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Models;

namespace Meetmind.Infrastructure.Calendar;

public class FakeCalendarService : ICalendarService
{
    public Task<List<UpcomingMeeting>> GetTodayMeetingsAsync(string token, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return Task.FromResult(new List<UpcomingMeeting>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Daily Standup",
                StartUtc = now.AddMinutes(15),
                EndUtc = now.AddMinutes(45),
                Source = "google",
                Participants = ["alice@example.com", "bob@example.com"]
            }
        });
    }

    public Task<List<UpcomingMeeting>> GetNextWeekMeetingsAsync(string token, CancellationToken ct)
        => Task.FromResult(new List<UpcomingMeeting>());
}