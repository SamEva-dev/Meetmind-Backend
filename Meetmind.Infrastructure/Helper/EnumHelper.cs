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
            SummaryModelType.BartLargeCnn => "facebook/bart-large-cnn",
            SummaryModelType.BartBaseFrench => "cmarkea/bart-base-french-summarizer",
            SummaryModelType.Bert2BertSpanish => "mrm8488/bert2bert_shared-spanish-finetuned-summarization",
            SummaryModelType.Mt5German => "ml6team/mt5-small-german-finetune-mlsum",
            SummaryModelType.Bert2BertItalian => "mrm8488/bert2bert_shared-italian-finetuned-summarization",
            SummaryModelType.BartBasePortuguese => "cmarkea/bart-base-portuguese-summarizer",
            _ => value.ToString().ToLowerInvariant()
        };
    }
}
