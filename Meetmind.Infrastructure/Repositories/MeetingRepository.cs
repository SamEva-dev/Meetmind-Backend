
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly MeetMindDbContext _dbContext;
    private readonly IMapper _mapper;
    public MeetingRepository(MeetMindDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public Task<MeetingDto?> GetMeetingById(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Meetings
            .AsNoTracking()
            .Where(m => m.Id == id)
            .ProjectTo<MeetingDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<MeetingDto>> GetMeetingToday(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        return _dbContext.Meetings
            .AsNoTracking()
            .Where(m => m.StartUtc.Date == today)
            .ProjectTo<MeetingDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
