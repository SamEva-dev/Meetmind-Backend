
using System.ComponentModel;


namespace Meetmind.Domain.Enums;

public enum SummaryModelType
{
    [Description("Modèle par défaut (BART fine-tuné sur le français)")]
    Auto,

    [Description("Modèle anglais généraliste (Facebook BART Large CNN)")]
    BartLargeCnn,

    [Description("Modèle français (BART fine-tuné sur le français)")]
    BartBaseFrench,

    [Description("Modèle espagnol (BERT2BERT fine-tuné sur l'espagnol)")]
    Bert2BertSpanish,

    [Description("Modèle allemand (MT5 fine-tuné sur le allemand)")]
    Mt5German,

    [Description("Modèle italien (BERT2BERT fine-tuné sur l'italien)")]
    Bert2BertItalian,

    [Description("Modèle portugais (BART fine-tuné sur le portugais)")]
    BartBasePortuguese
}
