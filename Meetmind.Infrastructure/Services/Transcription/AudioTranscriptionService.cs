using System.Text.Json;
using Meetmind.Application.Dto;
using Meetmind.Infrastructure.Helper;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Meetmind.Domain.Models;
using Meetmind.Application.Helper;
using Meetmind.Application.Services.Notification;
using NAudio.Wave;
using Google.Apis.Calendar.v3.Data;

namespace Meetmind.Infrastructure.Services.Transcription;

public class AudioTranscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AudioTranscriptionService> _logger;
    private readonly INotificationService _recordingNotifierService;
    private readonly MeetMindDbContext _db;

    public AudioTranscriptionService(HttpClient httpClient,
        ILogger<AudioTranscriptionService> logger,
        INotificationService recordingNotifierService,
        MeetMindDbContext db)
    {
        _httpClient = httpClient;
        _logger = logger;
        _db = db;
        _recordingNotifierService = recordingNotifierService;
    }

    public async Task<TranscriptionDto> TranscribeAsync(MeetingEntity meeting, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Début de la transcription pour la réunion {MeetingId}", meeting.Id);
            using var form = new MultipartFormDataContent();
            using var audioStream = File.OpenRead(meeting.AudioPath);

            var setting = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);

            form.Add(new StreamContent(audioStream), "audio", Path.GetFileName(meeting.AudioPath));
            form.Add(new StringContent(setting.Language.ToString()), "language");
            form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperModelType)), "whisper_model");
            form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperDeviceType)), "whisper_device");
            form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperComputeType)), "whisper_compute_type");
            form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.DiarizationModelType)), "diarization_model");
            form.Add(new StringContent(MeetingKey.ACCESS_TOKEN ?? ""), "hf_token");

            var response = await _httpClient.PostAsync("transcribe", form, ct);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("Transcription worker error: " + content);

            var result = JsonSerializer.Deserialize<TranscriptionDto>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var transcription = new TranscriptionEntity
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

            await _db.Transcriptions.AddAsync(transcription, ct);

            await _db.SaveChangesAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transcription for meeting {MeetingId}", meeting.Id);
            throw;
        }
    }

    public async Task ProcessChunkAsync(Guid meetingId, string chunkPath, SettingsEntity setting, CancellationToken ct)
    {
        try
        {
            if (!File.Exists(chunkPath))
                return;
            

            // 2. Appelle l’API Python (comme montré précédemment)
            using var client = new HttpClient();
        using var form = new MultipartFormDataContent();
            using var audioStream = File.OpenRead(chunkPath);

            form.Add(new StreamContent(audioStream), "audio", Path.GetFileName(chunkPath));

            form.Add(new StringContent(setting.Language.ToString()), "language");
        form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperModelType)), "whisper_model");
        form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperDeviceType)), "whisper_device");
        form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperComputeType)), "whisper_compute_type");
        form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.DiarizationModelType)), "diarization_model");
        form.Add(new StringContent(MeetingKey.ACCESS_TOKEN ?? ""), "hf_token");

        var response = await _httpClient.PostAsync("transcribe_chunk", form, ct);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TranscriptionDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //var transcription = await _db.Transcriptions
        //    .Include(t => t.Segments)
        //    .FirstOrDefaultAsync(t => t.MeetingId == meetingId, ct);
        //if (result == null)
        //{
        //    transcription = new TranscriptionEntity
        //    {
        //        MeetingId = meetingId,
        //        Tilte = "", // Renseigne selon ta logique
        //        SourceFile = chunkPath,
        //        Text = result.Text,
        //        Language = result.Language,
        //        LanguageProbability = result.Language_probability,
        //        Duration = result.Duration,
        //        CreatedAt = DateTime.UtcNow,
        //        Speakers = result.Speakers ?? new List<string>(),
        //        Segments = new List<TranscriptionSegment>()
        //    };
        //    //_db.Transcriptions.Add(transcription);
        //}
        //else
        //{
        //    // On concatène le texte live
        //    transcription.Text += " " + result.Text;
        //    transcription.Speakers = result.Speakers ?? transcription.Speakers;
        //}

        //// Ajoute les segments
        //if (result.Segments != null)
        //{
        //    foreach (var s in result.Segments)
        //    {
        //        // Option : vérifie que ce segment n'existe pas déjà (évite les doublons si besoin)
        //        transcription.Segments.Add(new TranscriptionSegment
        //        {
        //            TranscriptionId = transcription.Id,
        //            Speaker = s.Speaker,
        //            Text = s.Text,
        //            Start = s.Start,
        //            End = s.End
        //        });
        //    }
        //}

       // await _db.SaveChangesAsync(ct);

        //await _recordingNotifierService.NotifyLiveTranscriptionAsync(transcription.Text, ct);

            // 4. Optionnel: supprime le chunk
            // File.Delete(tmpPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}

