using Google.Apis.Calendar.v3.Data;
using MediatR;
using Meetmind.Application.Command.Transcription;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

namespace Meetmind.Infrastructure.Services.Recording;

public class AudioRecordingRouterService : IAudioRecordingService
{
    private readonly IReadOnlyList<IAudioRecordingService> _services;
    private readonly ISettingsRepository _settingsRepo;

    private readonly MeetMindDbContext _db;
    private readonly IMediator _mediator;
    private readonly ILogger<AudioRecordingRouterService> _logger;

    public AudioRecordingType BackendType
        => throw new NotSupportedException();

    public AudioRecordingRouterService(
        ISettingsRepository settingsRepo,
        NativeAudioRecordingService native,
        ProcessAudioRecordingService process,
        MeetMindDbContext db,
        IMediator mediator,
        ILogger<AudioRecordingRouterService> logger)
    {
        _settingsRepo = settingsRepo;
        _services = new List<IAudioRecordingService> { native, process};
        _db = db;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task StartAsync(Guid meetingId, string title, CancellationToken ct)
    {
        _logger.LogInformation("Démarrage de l'enregistrement pour la réunion {Id}", meetingId);
        var backend = (await _settingsRepo.GetAllAsync(ct))?.AudioRecordingType ?? AudioRecordingType.Native;
        var service = _services.FirstOrDefault(s => s.BackendType == backend && s.GetType() != typeof(AudioRecordingRouterService))
                      ?? throw new InvalidOperationException("Aucun service d'enregistrement audio trouvé !");

        await service.StartAsync(meetingId, title, ct);
    }

    public async Task<string> StopAsync(Guid meetingId, CancellationToken ct)
    {
        _logger.LogInformation("Arrêt de l'enregistrement pour la réunion {Id}", meetingId);
        var backend = (await _settingsRepo.GetAllAsync(ct))?.AudioRecordingType ?? AudioRecordingType.Native;
        var service = _services.FirstOrDefault(s => s.BackendType == backend && s.GetType() != typeof(AudioRecordingRouterService))
                      ?? throw new InvalidOperationException("Aucun service d'enregistrement audio trouvé !");

        
        var result =  await service.StopAsync(meetingId, ct);
        // on appel le transcriptoonHandler
        var settings = await _db.Settings
                            .AsNoTracking()
                            .FirstOrDefaultAsync(ct);
        if (!string.IsNullOrWhiteSpace(result) && settings != null && settings.AutoTranscript)
        {
            _logger.LogInformation("▶️ Auto-Transcrip meeting {Id}", meetingId);
            await _mediator.Send(new TranscriptionCommand(meetingId), ct);
        }

        return result;
    }

    public async Task PauseAsync(Guid meetingId, CancellationToken ct)
    {
        var backend = (await _settingsRepo.GetAllAsync(ct))?.AudioRecordingType ?? AudioRecordingType.Native;
        var service = _services.FirstOrDefault(s => s.BackendType == backend && s.GetType() != typeof(AudioRecordingRouterService))
                      ?? throw new InvalidOperationException("Aucun service d'enregistrement audio trouvé !");

        await service.PauseAsync(meetingId,ct);
    }

    public async Task ResumeAsync(Guid meetingId, CancellationToken ct)
    {
        var backend = (await _settingsRepo.GetAllAsync(ct))?.AudioRecordingType ?? AudioRecordingType.Native;
        var service = _services.FirstOrDefault(s => s.BackendType == backend && s.GetType() != typeof(AudioRecordingRouterService))
                      ?? throw new InvalidOperationException("Aucun service d'enregistrement audio trouvé !");

        await service.ResumeAsync(meetingId, ct);
    }

    public static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    // Pour assainir le nom du fichier
    private static string SanitizeFileName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(input.Where(c => !invalid.Contains(c)));
    }

}
