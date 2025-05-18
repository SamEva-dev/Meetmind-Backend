
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Google.Apis.Calendar.v3.Data;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Domain.Entities;
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
        return _dbContext.Set<MeetingEntity>()
            .AsNoTracking()
            .Where(m => m.Id == id)
            .ProjectTo<MeetingDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<MeetingEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<MeetingEntity>()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public Task<List<MeetingDto>> GetMeetingToday(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        return _dbContext.Set<MeetingEntity>()
            .AsNoTracking()
            .Where(m => m.StartUtc.Date == today)
            .ProjectTo<MeetingDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(MeetingDto meeting, CancellationToken cancellationToken)
    {
        _dbContext.Meetings.Remove(_mapper.Map<MeetingEntity>(meeting));
        await Task.CompletedTask;
    }

    public async Task ApplyAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
