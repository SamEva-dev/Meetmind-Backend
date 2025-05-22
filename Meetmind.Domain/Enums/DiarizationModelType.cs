
using System.ComponentModel;

namespace Meetmind.Domain.Enums;

public enum DiarizationModelType
{
    [Description("Modèle officiel pyannote/speaker-diarization-3.1")]
    SpeakerDiarization31,

    [Description("Modèle communautaire ou custom (préciser nom complet HuggingFace)")]
    Custom,

    [Description("Modèle officiel pyannote/pyannote/segmentation-3.0")]
    Segmentation30,

    [Description("Modèle officiel pyannote/pyannote/segmentation")]
    Segmentation,
}