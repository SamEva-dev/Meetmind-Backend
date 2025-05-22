
using System.ComponentModel;

namespace Meetmind.Domain.Enums;

public enum WhisperDeviceType
{
    [Description("CPU (processeur – compatible partout)")]
    Cpu,

    [Description("CUDA (GPU Nvidia – accélération, rapide)")]
    Cuda,

    [Description("Auto (détecte GPU, sinon CPU)")]
    Auto
}
