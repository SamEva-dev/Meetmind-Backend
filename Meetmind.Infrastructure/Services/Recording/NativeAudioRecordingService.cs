
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using Azure.Core;
using Google.Apis.Calendar.v3.Data;
using MediatR;
using Meetmind.Application.Dto;
using Meetmind.Application.Helper;
using Meetmind.Application.Services;
using Meetmind.Application.Services.Notification;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;
using Meetmind.Domain.Models;
using Meetmind.Infrastructure.Database;
using Meetmind.Infrastructure.Helper;
using Meetmind.Infrastructure.Services.Transcription;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using NAudio.Wave;

namespace Meetmind.Infrastructure.Services.Recording;

public class NativeAudioRecordingService : IAudioRecordingService
{
    private readonly ILogger<NativeAudioRecordingService> _logger;
    private readonly MeetMindDbContext _db;
    private readonly INotificationService _recordingNotifierService;
    private readonly AudioTranscriptionService _audioTranscriptionService;
    private readonly long TAILLE_CHUNK_EN_BYTES = 16000 * 2 * 5; // 5 secondes en mono 16kHz (16000 échantillons/s, 2 octets par échantillon)

    private WaveInEvent? _waveIn;
    private WaveFileWriter? _currentWriter;
    private System.Timers.Timer? _chunkTimer;
    private string? _currentChunkPath;
    private int _chunkIndex = 0;
    private bool _liveMode = false;
    private readonly object _writerLock = new object();


    public AudioRecordingType BackendType => AudioRecordingType.Native;

    // On mappe chaque enregistrement à sa session WaveInEvent
    private static readonly ConcurrentDictionary<Guid, (WaveInEvent Recorder, WaveFileWriter Writer)> _sessions = new();
    private static readonly ConcurrentDictionary<Guid, List<string>> _audioFragments = new();

    public NativeAudioRecordingService(MeetMindDbContext db,
        ILogger<NativeAudioRecordingService> logger,
        INotificationService recordingNotifierService,
        AudioTranscriptionService audioTranscriptionService
        )
    {
        _logger = logger;
        _db = db;
        _recordingNotifierService = recordingNotifierService;
        _audioTranscriptionService = audioTranscriptionService;
    }

    public Task StartAsync(Guid meetingId, string filePath, CancellationToken ct)
    {
        _logger.LogInformation("Démarrage de l'enregistrement pour la réunion {Id}", meetingId);
        _audioFragments.TryAdd(meetingId, new List<string>());
        return StartFragment(meetingId, filePath, ct);
    }

    private async Task StartFragment(Guid meetingId, string title, CancellationToken ct)
    {
        try
        {
            var audioPath = AudioFileHelper.GenerateAudioPath(title, meetingId);

            if (!_audioFragments.TryGetValue(meetingId, out var fragments))
                throw new InvalidOperationException("Session non initialisée");

            var settings = await _db.Settings.FirstOrDefaultAsync(ct);

            var directory = Path.GetDirectoryName(audioPath)!;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            fragments.Add(audioPath);

            _liveMode = settings != null && settings.LiveTranscriptionEnabled;

            _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 1) };

            if (_liveMode)
            {
                // --- MODE LIVE : un fichier wav par chunk ---
                StartNextLiveChunk(meetingId, title);

                _waveIn.DataAvailable += (s, a) =>
                {
                    if (_currentWriter != null)
                    {
                        _currentWriter?.Write(a.Buffer, 0, a.BytesRecorded);
                        _currentWriter?.Flush();
                    }
                    
                };

                // Timer pour couper le chunk toutes les X secondes
                _chunkTimer = new System.Timers.Timer(9000); // ex: 3s
                _chunkTimer.Elapsed += async (s, e) =>
                {
                    await CloseAndTranscribeCurrentChunkAsync(meetingId, settings, ct);
                    StartNextLiveChunk(meetingId, title);
                };
                _chunkTimer.Start();
            }
            else
            {
                // --- MODE CLASSIQUE : un seul fichier wav ---
                _currentChunkPath = audioPath;
                _currentWriter = new WaveFileWriter(audioPath, _waveIn.WaveFormat);
                _waveIn.DataAvailable += (s, a) => { _currentWriter.Write(a.Buffer, 0, a.BytesRecorded); _currentWriter.Flush(); };
            }

            _waveIn.RecordingStopped += async (s, a) =>
            {
                // En mode live, termine le chunk courant et timer
                if (_liveMode)
                {
                    _chunkTimer?.Stop();
                    await CloseAndTranscribeCurrentChunkAsync(meetingId, settings, ct);
                }
                // Dans tous les cas, ferme writer et waveIn
                _currentWriter?.Dispose();
                _waveIn?.Dispose();
            };

            if (!_sessions.TryAdd(meetingId, (_waveIn, _currentWriter!)))
            {
                _logger.LogWarning("Session d'enregistrement déjà existante pour la réunion {Id}", meetingId);
                _currentWriter?.Dispose();
                _waveIn?.Dispose();
                if (_audioFragments.TryGetValue(meetingId, out var fragmentsList))
                    fragmentsList.Remove(audioPath);
                else
                    _logger.LogWarning("Aucun fragment trouvé pour la réunion {Id}", meetingId);
                throw new InvalidOperationException("Erreur lors de l'ajout de la session d'enregistrement.");
            }

            _waveIn.StartRecording();

            var metadata = new AudioMetadata
            {
                MeetingId = meetingId,
                FilePath = audioPath,
                Title = title,
                StartUtc = DateTime.UtcNow,
            };
            _db.AudioMetadatas.Add(metadata);
            await _db.SaveChangesAsync(ct);

            await LogEventAsync(meetingId, "Start", $"AudioPath={audioPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage de l'enregistrement pour la réunion {Id}", meetingId);
            await LogEventAsync(meetingId, "Error", ex.Message);
        }
    }
    public async Task PauseAsync(Guid meetingId, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Mise en pause de l'enregistrement pour la réunion {Id}", meetingId);
            if (!_sessions.TryRemove(meetingId, out var session))
            {
                _logger.LogWarning("Aucune session d'enregistrement trouvée pour la réunion {Id}", meetingId);
                throw new InvalidOperationException("Aucun enregistrement actif à mettre en pause.");
            }
            var (waveIn, writer) = session;
            waveIn.StopRecording();
            
            await LogEventAsync(meetingId, "Pause", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise en pause l'enregistrement pour la réunion {Id}", meetingId);
            await LogEventAsync(meetingId, "Error", ex.Message);
        }
        
    }

    public async Task ResumeAsync(Guid meetingId, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Reprise de l'enregistrement pour la réunion {Id}", meetingId);
            if (!_audioFragments.ContainsKey(meetingId))
            {
                _logger.LogWarning("Aucune session d'enregistrement trouvée pour la réunion {Id}", meetingId);
                throw new InvalidOperationException("Aucun enregistrement actif à reprendre.");
            }
            var fragments = _audioFragments[meetingId];
            var pathBase = Path.GetDirectoryName(fragments.First()) ?? "Resources";
            var fileBase = Path.GetFileNameWithoutExtension(fragments.First()) ?? $"meeting_{meetingId}";
            var newFilePath = Path.Combine(pathBase, $"{fileBase}_{DateTime.UtcNow:HHmmssfff}.wav");
            await StartFragment(meetingId, newFilePath, ct);
            await LogEventAsync(meetingId, "Resume", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la reprise de l'enregistrement pour la réunion {Id}", meetingId);
            await LogEventAsync(meetingId, "Error", ex.Message);
        }
    }

    public async Task<string> StopAsync(Guid meetingId, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Arrêt de l'enregistrement pour la réunion {Id}", meetingId);
            if (_sessions.TryRemove(meetingId, out var session))
            {
                var (waveIn, writer) = session;
                waveIn.StopRecording();
            }
            // Concatène tous les fragments et supprime les fichiers temporaires
            if (!_audioFragments.TryRemove(meetingId, out var fragments))
            {
                _logger.LogWarning("Aucun fragment trouvé pour la réunion {Id}", meetingId);
                throw new InvalidOperationException("Aucun enregistrement actif à terminer.");
            }

            var pathBase = Path.GetDirectoryName(fragments.First()) ?? "Resources/audio";
            var fileBase = Path.GetFileNameWithoutExtension(fragments.First()) ?? $"meeting_{meetingId}";
            var outputFile = Path.Combine(pathBase, $"{fileBase}_final.wav");

            await ConcatWaveFilesAsync(fragments, outputFile);
            // Nettoyage fragments
            foreach (var frag in fragments) try { File.Delete(frag); } catch { }

            var metadata = _db.AudioMetadatas.FirstOrDefault(a => a.MeetingId == meetingId && a.EndUtc == null);
            if (metadata != null)
            {
                metadata.EndUtc = DateTime.UtcNow;
                // Calcule et enregistre la durée/fragments
                metadata.Duration = metadata.EndUtc - metadata.StartUtc;
                // metadata.FragmentCount = ...
                await _db.SaveChangesAsync(ct);
            }
            // Fix for CS0029: Cannot implicitly convert type 'double' to 'System.TimeSpan?'
            // Update the line to correctly calculate and assign a TimeSpan? value.

            await LogEventAsync(meetingId, "Stop", $"Fragments={_audioFragments.Count}");

            return outputFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'arret l'enregistrement pour la réunion {Id}", meetingId);
            await LogEventAsync(meetingId, "Error", ex.Message);
        }

        return null;
    }

    // Utilitaire pour concaténer les fragments (simplifié)
    private async Task ConcatWaveFilesAsync(List<string> inputFiles, string outputFile)
    {
        using var writer = new WaveFileWriter(outputFile, new WaveFormat(16000, 1));
        foreach (var file in inputFiles)
        {
            using var reader = new WaveFileReader(file);
            var buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                await writer.WriteAsync(buffer, 0, bytesRead);
        }
        writer.Flush();
    }

    private async Task LogEventAsync(Guid meetingId, string action, string? details = null, string? userId = null)
    {
        var log = new AudioEventLog
        {
            MeetingId = meetingId,
            Action = action,
            UtcTimestamp = DateTime.UtcNow,
            Details = details,
            UserId = userId
        };
        _db.AudioEventLogs.Add(log);
        await _db.SaveChangesAsync();
    }


    private void StartNextLiveChunk(Guid meetingId, string title)
    {
        lock (_writerLock)
        {
_currentWriter?.Dispose();
        _currentChunkPath = AudioFileHelper.GenerateAudioPath($"{title}{++_chunkIndex}", meetingId);
        _currentWriter = new WaveFileWriter(_currentChunkPath, new WaveFormat(16000, 1));
        }
            
    }

    private async Task CloseAndTranscribeCurrentChunkAsync(Guid meetingId, SettingsEntity settings, CancellationToken ct)
    {
        lock (_writerLock)
        {
_currentWriter?.Flush();
        _currentWriter?.Dispose();

        // Traite le chunk (envoie à l'API de transcription live, concatène en base, notifie SignalR)
        
        }
            if (!string.IsNullOrEmpty(_currentChunkPath))
        {
            await _audioTranscriptionService.ProcessChunkAsync(meetingId, _currentChunkPath, settings, ct);
        }
    }

}