
using Meetmind.Application.Dto;

namespace Meetmind.Application.Repositories;

public interface IMeetingRepository
{
    Task<MeetingDto?> GetMeetingById(Guid id, CancellationToken cancellationToken);
    Task<List<MeetingDto>> GetMeetingToday(CancellationToken cancellationToken);
}
