
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Google.Apis.Calendar.v3.Data;
using Meetmind.Application.Command.Meetings;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Domain.Models;
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
    }

    public async Task<Guid> CreateMeetingAsync(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = new MeetingEntity(request.Title, request.Start, request.End, request.ExternalId, request.ExternalSource);

        await  _dbContext.Meetings.AddAsync(meeting, cancellationToken);
        // projection read model
        await _dbContext.MeetingReadModels.AddAsync(new MeetingReadModel
        {
            Id = meeting.Id,
            Title = meeting.Title,
            StartUtc = meeting.StartUtc,
            EndUtc = meeting.EndUtc,
            Start = meeting.Start,
            End = meeting.End,
            State = meeting.State.ToString(),
            TranscriptPath = null,
            SummaryPath = null,
            ExternalId = meeting.ExternalId,
            ExternalSource = meeting.ExternalSource
        });
        // mise à jour du log en mémoire
        var lastLog = _dbContext.CalendarSyncLogs
            .Local
            .Where(l => l.Source == meeting.ExternalSource)
            .OrderByDescending(l => l.TimestampUtc)
            .FirstOrDefault();

        if (lastLog != null)
            lastLog.MeetingsCreated++;

        return meeting.Id;
    }

    public Task<MeetingDto?> GetAllAsync(int? count, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<List<MeetingEntity>> ListAsync(Expression<Func<MeetingEntity, bool>>? filter = null, bool tracking = false)
    {
        Expression<Func<MeetingEntity, bool>> predicate = filter == null ? t => true : _mapper.Map<Expression<Func<MeetingEntity, bool>>>(filter);

        var query = _dbContext.Set<MeetingEntity>()
            .Where(predicate)
            .OrderByDescending(m => m.EndUtc);

        return tracking ? await query.ToListAsync() : await query.AsNoTracking().ToListAsync();
    }

    public async Task<MeetingEntity?> SingleOrDefaultAsync(Expression<Func<MeetingEntity, bool>> filter, bool tracking = false)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AnyAsync(Expression<Func<MeetingEntity, bool>> filter)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<MeetingEntity>> ListPagedAsync(int page, int pageSize, Expression<Func<MeetingEntity, bool>>? filter = null, Func<IQueryable<MeetingEntity>, IOrderedQueryable<MeetingEntity>>? orderBy = null, bool tracking = false)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        IQueryable<MeetingEntity> query = _dbContext.Set<MeetingEntity>();

        if (filter != null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync();

        if (orderBy != null)
            query = orderBy(query);

        if (!tracking)
            query = query.AsNoTracking();

        var items = await query
            .OrderByDescending(m => m.EndUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MeetingEntity>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task ExecuteSqlRawAsync(ExecuteType type)
    {
        try
        {
            switch (type)
            {
                case ExecuteType.DELETE:
                    await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Meetings");
                    break;
                case ExecuteType.DROP:
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        " DROP TABLE IF EXISTS AudioEventLogs");
                    break;
                default:
                    break;
            }
           
        }
        catch (Exception ex)
        {

            throw;
        }
        
    }
}
