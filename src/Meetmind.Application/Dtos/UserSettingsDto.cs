
namespace Meetmind.Application.Dtos;

public class UserSettingsDto
{
    public bool AutoStartRecord { get; set; }
    public bool AutoTranscript { get; set; }
    public bool AutoSummarize { get; set; }
    public bool AutoTranslate { get; set; }
    public List<int> NotifyBeforeMinutes { get; set; } = new() { 10, 5, 1 };
}