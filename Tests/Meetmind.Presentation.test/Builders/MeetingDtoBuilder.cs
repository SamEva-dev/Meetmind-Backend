using Meetmind.Application.Dto;

namespace Meetmind.Presentation.test.Builders;

public class MeetingDtoBuilder
{
    private MeetingDto _dto;

    public MeetingDtoBuilder()
    {
        _dto = new MeetingDto
        {
            Id = Guid.NewGuid(),
            Title = "Sample Meeting",
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddHours(1),
            State = "Scheduled",
            TranscriptState = "NotAvailable",
            SummaryState = "NotAvailable",
            AudioPath = null,
            TranscriptPath = null,
            SummaryPath = null
        };
    }

    public MeetingDtoBuilder WithId(Guid id)
    {
        _dto = new MeetingDto
        {
            Id = id,
            Title = _dto.Title,
            StartUtc = _dto.StartUtc,
            EndUtc = _dto.EndUtc,
            State = _dto.State,
            TranscriptState = _dto.TranscriptState,
            SummaryState = _dto.SummaryState,
            AudioPath = _dto.AudioPath,
            TranscriptPath = _dto.TranscriptPath,
            SummaryPath = _dto.SummaryPath
        };
        return this;
    }

    public MeetingDtoBuilder WithTitle(string title)
    {
        _dto = new MeetingDto
        {
            Id = _dto.Id,
            Title = title,
            StartUtc = _dto.StartUtc,
            EndUtc = _dto.EndUtc,
            State = _dto.State,
            TranscriptState = _dto.TranscriptState,
            SummaryState = _dto.SummaryState,
            AudioPath = _dto.AudioPath,
            TranscriptPath = _dto.TranscriptPath,
            SummaryPath = _dto.SummaryPath
        };
        return this;
    }

    // ... Ajoute ici d'autres méthodes si besoin

    public MeetingDto Build() => _dto;

    public static List<MeetingDto> BuildList(int count = 2)
    {
        var list = new List<MeetingDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new MeetingDtoBuilder()
                .WithId(Guid.NewGuid())
                .WithTitle($"Meeting {i + 1}")
                .Build());
        }
        return list;
    }
}
