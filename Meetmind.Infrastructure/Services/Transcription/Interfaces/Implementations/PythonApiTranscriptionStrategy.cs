using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Meetmind.Application.Dto;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Models;
using Meetmind.Infrastructure.Helper;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Services.Transcription.Interfaces.Implementations;

public class PythonApiTranscriptionStrategy : ITranscriptionStrategy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonApiTranscriptionStrategy> _logger;

    public PythonApiTranscriptionStrategy(HttpClient httpClient, ILogger<PythonApiTranscriptionStrategy> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TranscriptionDto> TranscribeAsync(Stream audioStream, SettingsEntity settings, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();

        form.Add(new StreamContent(audioStream), "audio", "chunk.wav");
        form.Add(new StringContent(settings.Language.ToString()), "language");
        form.Add(new StringContent(EnumHelper.GetEnumValueForPython(settings.WhisperModelType)), "whisper_model");
        form.Add(new StringContent(EnumHelper.GetEnumValueForPython(settings.WhisperDeviceType)), "whisper_device");
        form.Add(new StringContent(EnumHelper.GetEnumValueForPython(settings.WhisperComputeType)), "whisper_compute_type");
        form.Add(new StringContent(EnumHelper.GetEnumValueForPython(settings.DiarizationModelType)), "diarization_model");
        form.Add(new StringContent(MeetingKey.ACCESS_TOKEN ?? ""), "hf_token");

        var response = await _httpClient.PostAsync("transcribe_chunk", form, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Transcription worker error: " + content);

        return JsonSerializer.Deserialize<TranscriptionDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
