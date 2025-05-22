
using System.ComponentModel;

namespace Meetmind.Domain.Enums;

public enum WhisperComputeType
{
    [Description("Auto (choix optimal par le modèle)")]
    Default,

    [Description("int8 (CPU) – quantization, rapide, mémoire faible)")]
    Int8,

    [Description("int8_float16 (CPU/GPU) – quantization plus poussée)")]
    Int8Float16,

    [Description("int16 (rare, parfois sur CPU)")]
    Int16,

    [Description("float16 (GPU, mémoire réduite, très rapide sur CUDA)")]
    Float16,

    [Description("float32 (précision max, RAM élevée)")]
    Float32
}
