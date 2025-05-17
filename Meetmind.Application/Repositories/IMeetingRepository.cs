
using Meetmind.Application.Dto;

namespace Meetmind.Application.Repositories;

public interface IMeetingRepository : IDisposable
{
    Task ApplyAsync(CancellationToken cancellationToken);
    Task DeleteAsync(MeetingDto meeting, CancellationToken cancellationToken);
    Task<MeetingDto?> GetMeetingById(Guid id, CancellationToken cancellationToken);
    Task<List<MeetingDto>> GetMeetingToday(CancellationToken cancellationToken);
}
