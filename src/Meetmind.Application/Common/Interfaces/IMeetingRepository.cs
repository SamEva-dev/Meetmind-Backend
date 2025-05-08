using Meetmind.Application.Dtos;
using Meetmind.Domain.Entities;

namespace Meetmind.Application.Common.Interfaces;

public interface IMeetingRepository
{
    Task<Meeting?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<MeetingDto>> GetMeetingsTodayAsync(DateTime today, CancellationToken ct);
    Task<MeetingDto?> GetTranscription(Guid meetingId, CancellationToken ct);
}