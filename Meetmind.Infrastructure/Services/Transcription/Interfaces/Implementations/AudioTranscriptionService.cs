using System.Text.Json;
using Meetmind.Application.Dto;
using Meetmind.Infrastructure.Helper;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Meetmind.Domain.Models;
using Meetmind.Application.Services.Notification;

namespace Meetmind.Infrastructure.Services.Transcription.Interfaces.Implementations;

public class AudioTranscriptionService : IAudioTranscriptionService
{
    private readonly ITranscriptionStrategy _strategy;
    private readonly MeetMindDbContext _db;
    private readonly INotificationService _notifier;
    private readonly ILogger<AudioTranscriptionService> _logger;

    public AudioTranscriptionService(
        ITranscriptionStrategy strategy,
        MeetMindDbContext db,
        INotificationService notifier,
        ILogger<AudioTranscriptionService> logger)
    {
        _strategy = strategy;
        _db = db;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<TranscriptionDto> TranscribeMeetingAsync(MeetingEntity meeting, CancellationToken ct)
    {
        try
        {
            using var stream = File.OpenRead(meeting.AudioPath);
            var settings = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);
            var result = await _strategy.TranscribeAsync(stream, settings, ct);

            var entity = new TranscriptionEntity
            {
                MeetingId = meeting.Id,
                Tilte = meeting.Title,
                SourceFile = meeting.AudioPath,
                Text = result.Text,
                Language = result.Language,
                LanguageProbability = result.Language_probability,
                Duration = result.Duration,
                CreatedAt = DateTime.UtcNow,
                Segments = result.Segments?.Select(s => new TranscriptionSegment
                {
                    TranscriptionId = meeting.Id,
                    Start = s.Start,
                    End = s.End,
                    Text = s.Text,
                    Speaker = s.Speaker,
                }).ToList()
            };

            await _db.Transcriptions.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur de transcription pour la réunion {MeetingId}", meeting.Id);
            throw;
        }
    }

    public async Task TranscribeChunkAsync(Guid meetingId, string chunkPath, SettingsEntity settings, CancellationToken ct)
    {
        try
        {
            if (!File.Exists(chunkPath)) return;

            using var stream = File.OpenRead(chunkPath);
            var result = await _strategy.TranscribeAsync(stream, settings, ct);

            // facultatif : notifier / journaliser si besoin
            _logger.LogInformation("Chunk transcrit pour {MeetingId}", meetingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la transcription du chunk pour la réunion {MeetingId}", meetingId);
        }
    }
}
