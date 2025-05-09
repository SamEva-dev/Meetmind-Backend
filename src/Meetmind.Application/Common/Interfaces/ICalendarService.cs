
using Meetmind.Domain.Models;

namespace Meetmind.Application.Common.Interfaces;

public interface ICalendarService
{
    Task<List<UpcomingMeeting>> GetTodayMeetingsAsync(string userToken, CancellationToken ct);
    Task<List<UpcomingMeeting>> GetNextWeekMeetingsAsync(string userToken, CancellationToken ct);
}