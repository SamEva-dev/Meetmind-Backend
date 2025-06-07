using Meetmind.Application.Command.Meetings;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;

namespace Meetmind.Application.Repositories;

public interface IMeetingRepository : IRepository<MeetingEntity>
{
    Task DeleteAsync(MeetingDto meeting, CancellationToken cancellationToken);
    Task<MeetingDto?> GetMeetingById(Guid id, CancellationToken cancellationToken);
    Task<List<MeetingDto>> GetMeetingToday(CancellationToken cancellationToken);
    Task<MeetingEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateMeetingAsync(CreateMeetingCommand request, CancellationToken cancellationToken);
    Task<MeetingDto?> GetAllAsync(int? count, CancellationToken cancellationToken);
}
