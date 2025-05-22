
namespace Meetmind.Application.Dto;

public class TranscriptionDto
{
    public string Text { get; set; }

    public string Tilte { get; set; }
    public List<SegmentDto> Segments { get; set; }
    public string Language { get; set; }
    public double? Language_probability { get; set; }
    public double? Duration { get; set; }
    public int Segments_count { get; set; }
    public List<string> Speakers { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
    public string? OutputPath { get; set; }
}

public class SegmentDto
{
    public string Start { get; set; }
    public string End { get; set; }
    public string Text { get; set; }
    public string Speaker { get; set; }
}

