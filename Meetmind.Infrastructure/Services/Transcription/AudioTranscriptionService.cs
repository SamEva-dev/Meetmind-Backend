using System.Text.Json;
using Meetmind.Application.Dto;
using Meetmind.Infrastructure.Helper;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Services.Transcription;

public class AudioTranscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AudioTranscriptionService> _logger;
    private readonly MeetMindDbContext _db;
    private const string ACCESS_TOKEN = "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";

    public AudioTranscriptionService(HttpClient httpClient,
        ILogger<AudioTranscriptionService> logger,
        MeetMindDbContext db)
    {
        _httpClient = httpClient;
        _logger = logger;
        _db = db;
    }

    public async Task<TranscriptionDto> TranscribeAsync(MeetingEntity meeting, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Début de la transcription pour la réunion {MeetingId}", meeting.Id);
            using var form = new MultipartFormDataContent();
            using var audioStream = File.OpenRead(meeting.AudioPath);

            var setting = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);
            var test = setting.Language.ToString();
            form.Add(new StreamContent(audioStream), "audio", Path.GetFileName(meeting.AudioPath));
            form.Add(new StringContent(setting.Language.ToString()), "language");
            form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperModelType)), "whisper_model");
            form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperDeviceType)), "whisper_device");
            form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.WhisperComputeType)), "whisper_compute_type");
            form.Add(new StringContent(EnumHelper.GetEnumValueForPython(setting.DiarizationModelType)), "diarization_model");
            form.Add(new StringContent(ACCESS_TOKEN ?? ""), "hf_token");

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
}

