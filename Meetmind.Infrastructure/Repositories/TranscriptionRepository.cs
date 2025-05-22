using AutoMapper;
using AutoMapper.QueryableExtensions;
using Meetmind.Application.Dto;
using Meetmind.Application.Repositories;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Repositories
{
    public class TranscriptionRepository : ITranscriptionRepository
    {
        private readonly MeetMindDbContext _db;
        private readonly IMapper _mapper;

        public TranscriptionRepository(MeetMindDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task AddTransition(TranscriptionDto dto, CancellationToken cancellationToken)
        {
            var transcription = _mapper.Map<TranscriptionEntity>(dto);
            await _db.Transcriptions.AddAsync(transcription, cancellationToken);
        }

        public async Task DeleteTransition(TranscriptionDto dto, CancellationToken cancellationToken)
        {
            var transcription = _mapper.Map<TranscriptionEntity>(dto);
            _db.Segments.RemoveRange(transcription.Segments);
            _db.Transcriptions.Remove(transcription);
        }

        public async Task<List<TranscriptionDto>> GetTranscriptionAsync(CancellationToken cancellationToken)
        {
            var query = await _db.Transcriptions
                .AsNoTracking()
                .Include(t => t.Segments)
                .OrderByDescending(t => t.CreatedAt)
                .ProjectTo<TranscriptionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return query;
        }

        public async Task<TranscriptionDto> GetTranscriptionByIdAsync(Guid meetingId, CancellationToken cancellationToken)
        {
            var query = await _db.Transcriptions
                .AsNoTracking()
                .Include(t => t.Segments)
                .Where(m => m.Id == meetingId)
                .ProjectTo<TranscriptionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return query;
        }
    }
}
