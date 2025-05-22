
using Meetmind.Application.Dto;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Meetmind.Application.Services.Pdf;

public class TranscriptPdfDocument : IDocument
{
    private readonly TranscriptionDto _data;

    public TranscriptPdfDocument(TranscriptionDto data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Header()
                .Column(col =>
                {
                    col.Item().Text(_data.Tilte).FontSize(18).Bold();
                    col.Item().Text($"Langue: {_data.Language}");
                    col.Item().Text($"Participants: {string.Join(", ", _data.Speakers)}");
                });
            page.Content()
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(80); // Heure début
                        columns.ConstantColumn(80); // Heure fin
                        columns.RelativeColumn(0.2f); // Speaker
                        columns.RelativeColumn(1.0f); // Texte
                    });
                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Text("Début").SemiBold();
                        header.Cell().Text("Fin").SemiBold();
                        header.Cell().Text("Speaker").SemiBold();
                        header.Cell().Text("Texte").SemiBold();
                    });
                    // Segments
                    foreach (var seg in _data.Segments)
                    {
                        table.Cell().Text(seg.Start);
                        table.Cell().Text(seg.End);
                        table.Cell().Text(seg.Speaker);
                        table.Cell().Text(seg.Text);
                    }
                });
            page.Footer()
                .AlignCenter()
                .Text(txt =>
                {
                    txt.Span("Page ");
                    txt.CurrentPageNumber();
                    txt.Span(" / ");
                    txt.TotalPages();
                });
        });
    }
}