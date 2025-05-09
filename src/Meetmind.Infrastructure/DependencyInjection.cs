

using Meetmind.Application.Common.Interfaces;
using Meetmind.Infrastructure.Calendar;
using Meetmind.Infrastructure.Db;
using Meetmind.Infrastructure.Hubs;
using Meetmind.Infrastructure.Mapping;
using Meetmind.Infrastructure.Orchestration;
using Meetmind.Infrastructure.Realtime;
using Meetmind.Infrastructure.Repositories;
using Meetmind.Infrastructure.Search;
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

           // services.AddAutoMapper(typeof(UserSettingsProfile));
            services.AddAutoMapper((serviceProvider, cfg) =>
            {
                //  cfg.AddExpressionMapping();
                // cfg.AddCollectionMappers();
                cfg.AddProfile<UserSettingsProfile>();
            }, new System.Reflection.Assembly[0]);

            services.AddSingleton<INotifier, NotifyHubNotifier>();
            services.AddSingleton<TranscriptSemanticAnalyzer>();
            services.AddSingleton<INotificationService, SignalRNotificationService>();
            services.AddSingleton<IMeetingOrchestratorManager, MeetingOrchestratorManager>();



            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
            services.AddScoped<IWhisperService, WhisperPythonService>();
            services.AddScoped<ISummaryService, FakeSummaryService>();
            services.AddScoped<ISummaryService, MarkdownSummaryService>();
            services.AddScoped<ISearchService, SearchIndexService>();
            services.AddScoped<ICalendarService, FakeCalendarService>();
            services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();




            services.AddHostedService<TranscriptionWorker>();
            services.AddHostedService<SummaryWorker>();
            services.AddHostedService<MeetingSchedulerWorker>();





            return services;
        }
    }
}
