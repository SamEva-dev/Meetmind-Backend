using AutoMapper;
using AutoMapper.QueryableExtensions;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly MeetMindDbContext _dbContext;
        private readonly IMapper _mapper;
        public SettingsRepository(MeetMindDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task SaveAsync(SettingsEntity existingSettings, CancellationToken cancellationToken)
        {
            _dbContext.Settings.Add(existingSettings);

        }

        public Task<SettingsDto> GetAllAsync(CancellationToken cancellationToken)
        {
            return _dbContext.Settings
                .AsNoTracking()
                .ProjectTo<SettingsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Settings.FirstOrDefaultAsync(cancellationToken);
            if (existing != null)
            {
                _dbContext.Settings.Remove(existing);
            }
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
}
