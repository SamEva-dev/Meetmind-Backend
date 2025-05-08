using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Enums;
using Meetmind.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetmind.Infrastructure.Worker;

public class SummaryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SummaryWorker> _logger;
    private const int IntervalSeconds = 10;

    public SummaryWorker(IServiceScopeFactory scopeFactory, ILogger<SummaryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SummaryWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();
                var summarizer = scope.ServiceProvider.GetRequiredService<ISummaryService>();

                var pending = await db.Meetings
                    .Where(m => m.SummaryState == SummaryState.Queued)
                    .ToListAsync(stoppingToken);

                foreach (var meeting in pending)
                {
                    try
                    {
                        meeting.MarkSummaryProcessing();
                        await db.SaveChangesAsync(stoppingToken);

                        var summaryPath = await summarizer.GenerateSummaryAsync(meeting.Id, stoppingToken);
                        meeting.MarkSummaryCompleted(summaryPath);
                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Summary completed for Meeting {Id}", meeting.Id);
                    }
                    catch (Exception ex)
                    {
                        meeting.MarkSummaryFailed();
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogError(ex, "Failed to summarize Meeting {Id}", meeting.Id);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "SummaryWorker crashed");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}