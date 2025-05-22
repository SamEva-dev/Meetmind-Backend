
using System.Text;
using Meetmind.Application.Dto;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Database;
using Meetmind.Infrastructure.Protos;
using Meetmind.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Services.Transcription;

public class InteropTranscriptionService : ITranscriptionService
{
    public TranscriptionType BackendType => TranscriptionType.Interop;

    private readonly ILogger<InteropTranscriptionService> _logger;
    private readonly MeetMindDbContext _db;
    private readonly UnitOfWork _unitOfWork;

    public InteropTranscriptionService(ILogger<InteropTranscriptionService> logger)
    {
        _logger = logger;
       // _db = db;
    }

    public async Task<TranscriptionDto> TranscribeAsync(MeetingEntity meeting, CancellationToken cancellationToken)
    {
        return await  Task.Run(() =>
        {
            // (taille max à adapter selon la lib, ex : 4096 ou 16384)
            var output = new StringBuilder(16384);

            int code = WhisperInterop.transcribe(meeting.AudioPath, output, output.Capacity);

            if (code != 0)
            {
                _logger.LogError("WhisperInterop.transcribe a retourné une erreur {Code}", code);
                throw new Exception($"Interop transcription failed with code {code}");
            }

            var transcription = new TranscriptionEntity
            {
                MeetingId = Guid.NewGuid(),
                SourceFile = meeting.AudioPath,
                Text = output.ToString(),
                Language = default,
                LanguageProbability = default,
                Duration = null,
                CreatedAt = DateTime.UtcNow
            };

             _db.Transcriptions.AddAsync(transcription, cancellationToken);

            _db.SaveChangesAsync(cancellationToken);

            return new TranscriptionDto
            {
                Text = output.ToString(),
            };
        }, cancellationToken);
    }

}
