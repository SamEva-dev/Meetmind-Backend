using Meetmind.Application.Services;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Workers;

internal class SummarizeWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TranscriptionWorker> _logger;

    public SummarizeWorker(IServiceScopeFactory scopeFactory,
        ILogger<TranscriptionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSummarizeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans SummarizeWorker");
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }

    private async Task ProcessSummarizeAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();
        var _summService = scope.ServiceProvider.GetRequiredService<ISummarizeService>();

        var test = await db.Meetings.ToListAsync(ct);
        var meetingsToSummarize = await db.Meetings
            .Where(m => m.State == MeetingState.Done && m.SummaryState == SummaryState.Queued)
            .ToListAsync(ct);

        foreach (var meeting in meetingsToSummarize)
        {
            try
            {
                meeting.MarkSummaryProcessing();
                await db.SaveChangesAsync(ct);

                var summarizePath = await _summService.SummarizeAsync(meeting, ct);

                meeting.MarkSummaryCompleted(summarizePath);
                _logger.LogInformation("✅ Résumé complétée pour {Id}", meeting.Id);
            }
            catch (Exception ex)
            {
                meeting.MarkSummaryFailed();
                _logger.LogError(ex, "❌ Échec du résumé pour {Id}", meeting.Id);
            }

            await db.SaveChangesAsync(ct);
        }
    }
}