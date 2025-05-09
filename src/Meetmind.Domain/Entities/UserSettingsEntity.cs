using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetmind.Domain.Entities;

public class UserSettingsEntity
{
    public Guid Id { get; set; } // lié à l’utilisateur (ou config globale temporaire)
    public bool AutoStartRecord { get; set; }
    public bool AutoTranscript { get; set; }
    public bool AutoSummarize { get; set; }
    public bool AutoTranslate { get; set; }
    public string NotifyBeforeMinutesJson { get; set; } = "[10,5,1]";

    public List<int> GetNotifyBeforeMinutes() =>
        System.Text.Json.JsonSerializer.Deserialize<List<int>>(NotifyBeforeMinutesJson) ?? new() { 10, 5, 1 };
}