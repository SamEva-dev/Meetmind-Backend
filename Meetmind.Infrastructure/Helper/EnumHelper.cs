using Meetmind.Domain.Enums;

namespace Meetmind.Infrastructure.Helper;

public static class EnumHelper
{
    public static string GetEnumValueForPython(Enum value)
    {
        // Ce switch donne le "string attendu par le worker"
        return value switch
        {
            WhisperModelType.Tiny => "tiny",
            WhisperModelType.Base => "base",
            WhisperModelType.Small => "small",
            WhisperModelType.Medium => "medium",
            WhisperModelType.LargeV2 => "large-v2",
            WhisperModelType.LargeV3 => "large-v3",
            WhisperDeviceType.Cpu => "cpu",
            WhisperDeviceType.Cuda => "cuda",
            WhisperDeviceType.Auto => "auto",
            WhisperComputeType.Default => "default",
            WhisperComputeType.Int8 => "int8",
            WhisperComputeType.Int8Float16 => "int8_float16",
            WhisperComputeType.Int16 => "int16",
            WhisperComputeType.Float16 => "float16",
            WhisperComputeType.Float32 => "float32",
            DiarizationModelType.SpeakerDiarization31 => "pyannote/speaker-diarization-3.1",
            DiarizationModelType.Segmentation30 => "pyannote/segmentation-3.0",
            DiarizationModelType.Segmentation => "pyannote/segmentation",
            _ => value.ToString().ToLowerInvariant()
        };
    }
}
