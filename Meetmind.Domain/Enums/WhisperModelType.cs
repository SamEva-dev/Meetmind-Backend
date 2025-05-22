
using System.ComponentModel;

namespace Meetmind.Domain.Enums;

public enum WhisperModelType
{
    [Description("Tiny (32M params) – rapide, qualité basse")]
    Tiny,

    [Description("Base (74M params) – rapide, qualité correcte")]
    Base,

    [Description("Small (244M params) – bon compromis vitesse/qualité")]
    Small,

    [Description("Medium (769M params) – haute qualité, plus lent")]
    Medium,

    [Description("Large-v2 (1550M params) – meilleure qualité, lent, multilingue")]
    LargeV2,

    [Description("Large-v3 (1550M params) – plus robuste, multilingue")]
    LargeV3
}