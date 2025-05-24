using Meetmind.Application.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Meetmind.Application.Services.Pdf;

public class SummaryPdfDocument : IDocument
{
    private readonly string _title;
    private readonly string _summary;

    public SummaryPdfDocument(SummarizeDto summarizeDto)
    {
        _title = summarizeDto.MeetingTitle ?? "Résumé de la réunion";
        _summary = summarizeDto.SummaryText ?? "";
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(x => x.FontSize(12));
            page.Content()
                .Column(col =>
                {
                    col.Item().Text(_title).FontSize(18).Bold().ParagraphSpacing(30);
                    col.Item().Text("Résumé de la réunion").FontSize(14).Italic().ParagraphSpacing(20);
                    col.Item().Text(_summary).FontSize(12);
                });
            page.Footer()
                .AlignCenter()
                .Text(x =>
                {
                    x.Span("Généré par MeetMind - ").FontSize(10);
                    x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
        });
    }
}