using Meetmind.Application.Repositories;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Repositories
{
    public class AudioFragmentRepository : IAudioFragmentRepository
    {
        private readonly MeetMindDbContext _dbContext;

        public AudioFragmentRepository(MeetMindDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddFragment(AudioMetadata fragment, CancellationToken cancellationToken)
        {
            await _dbContext.AddAsync(fragment, cancellationToken);   
        }

        public async Task DeleteAsync(AudioMetadata fragment, CancellationToken cancellationToken)
        {
             _dbContext.AudioMetadatas.Remove(fragment);
            await Task.CompletedTask;
        }

        public Task<AudioMetadata> GetFragmentIdAsync(Guid meetingId, CancellationToken cancellationToken)
        {
            return _dbContext.AudioMetadatas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MeetingId == meetingId, cancellationToken);
        }
    }
}
