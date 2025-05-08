

using Meetmind.Application.Common.Interfaces;
using Meetmind.Infrastructure.Db;
using Meetmind.Infrastructure.Hubs;
using Meetmind.Infrastructure.Repositories;
using Meetmind.Infrastructure.Summary;
using Meetmind.Infrastructure.Transcription;
using Meetmind.Infrastructure.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meetmind.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
           

            var dbPath = config.GetConnectionString("Sqlite") ?? "Data/meetmind.db";
            services.AddDbContext<MeetMindDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            services.AddSingleton<INotifier, NotifyHubNotifier>();
            services.AddSingleton<TranscriptSemanticAnalyzer>();


            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
            services.AddScoped<IWhisperService, WhisperPythonService>();
            services.AddScoped<ISummaryService, FakeSummaryService>();
            services.AddScoped<ISummaryService, MarkdownSummaryService>();



            services.AddHostedService<TranscriptionWorker>();
            services.AddHostedService<SummaryWorker>();




            return services;
        }
    }
}
