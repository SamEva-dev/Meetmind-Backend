using Meetmind.Application.Connectors;
using Meetmind.Application.Repositories;
using Meetmind.Application.Services;
using Meetmind.Domain.Events.Interface;
using Meetmind.Infrastructure.Connectors;
using Meetmind.Infrastructure.Database;
using Meetmind.Infrastructure.Events;
using Meetmind.Infrastructure.Hubs;
using Meetmind.Infrastructure.Mapping;
using Meetmind.Infrastructure.Repositories;
using Meetmind.Infrastructure.Services;
using Meetmind.Infrastructure.Services.Recording;
using Meetmind.Infrastructure.Services.Transcription;
using Meetmind.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper.Extensions.EnumMapping;
using AutoMapper.EquivalencyExpression;
using Meetmind.Infrastructure.Services.Summarize;
using Meetmind.Application.Services.Notification;
using Meetmind.Application.Services.AudioRecorder;
using Meetmind.Infrastructure.Services.Transcription.Interfaces.Implementations;
using Meetmind.Infrastructure.Services.Recording.Interfaces.Implementations;
using Meetmind.Infrastructure.Services.Recording.Interfaces;
using Meetmind.Infrastructure.Services.Transcription.Interfaces;

namespace Meetmind.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var dbPath = config.GetConnectionString("Sqlite") ?? "Data/meetmind.db";
            services.AddDbContext<MeetMindDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            var workerHost = config["FastAPI:Host"] ?? "127.0.0.1";
            var workerPort = config["FastAPI:Port"] ?? "8000";

            services.AddAutoMapper((serviceProvider, cfg) =>
            {
                 // cfg.AddExpressionMapping();
                cfg.AddCollectionMappers();
                //cfg.AddProfile<MeetingProfile>();
                cfg.AddProfile<TranscriptionProfile>();
                cfg.AddProfile<MeetingProfile>();   
                cfg.AddProfile<SettingsProfile>();
                cfg.AddProfile<CalendarSyncLogProfile>();
            }, new System.Reflection.Assembly[0]);

            services.AddHostedService<CalendarWorker>();
            services.AddHostedService<TranscriptionWorker>();
            services.AddHostedService<SummarizeWorker>();

            //services.AddGrpcClient<WhisperTranscription.WhisperTranscriptionClient>(options =>
            //{
            //    options.Address = new Uri("https://localhost:5001");
            //})
            //    .ConfigurePrimaryHttpMessageHandler(() =>
            //new HttpClientHandler
            //{
            //    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            //});
            services.AddScoped<GrpcTranscriptionService>();



            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<ITranscriptionRepository,TranscriptionRepository>();
            services.AddScoped<INotificationService, SignalRNotificationService>();
            services.AddScoped<IMeetingCreatorService, MeetingCreatorService>();
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddScoped<IOutlookAuthService, OutlookAuthService>();
            services.AddScoped<ICalendarConnector, OutlookCalendarConnector>();
            services.AddScoped<ICalendarConnector, GoogleCalendarConnector>();
            services.AddScoped<ICalendarSyncLogRepository, CalendarSyncLog>();
            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IOutlookTokenService, OutlookTokenService>();
            services.AddScoped<IAudioFragmentRepository, AudioFragmentRepository>();

            services.AddScoped<IAudioRecordingService, NativeAudioRecordingService>();
            services.AddScoped<IAudioRecordingService, NativeAudioRecordingService>();

            services.AddScoped<IWaveInFactory, WaveInFactory>();
            services.AddScoped<IAudioFileStorage, AudioFileStorage>();
            services.AddSingleton<IAudioSessionManager, AudioSessionManager>();

            services.AddScoped<IAudioTranscriptionService, AudioTranscriptionService>();
            services.AddScoped<IAudioSession, NativeAudioSession>();

            services.AddScoped<ITranscriptionStrategy, PythonApiTranscriptionStrategy>();

            services.AddScoped<ISummarizeService, SummarizeService>();

            services.AddScoped<GrpcTranscriptionService>();
            services.AddScoped<ProcessTranscriptionService>();
            services.AddScoped<InteropTranscriptionService>();

            services.AddScoped<NativeAudioRecordingService>();
            services.AddScoped<ProcessAudioRecordingService>();

            services.AddHttpClient<AudioTranscriptionService>(client =>
            {
                client.BaseAddress = new Uri($"http://{workerHost}:{workerPort}/");
            });

            services.AddHttpClient<AudioSummarizeService>(client =>
            {
                client.BaseAddress = new Uri($"http://{workerHost}:{workerPort}/");
            });


            services.AddScoped<AudioRecordingRouterService>();

            services.AddScoped<TranscriptionRouterService>();

            services.AddScoped<ITranscriptionService>(sp => sp.GetRequiredService<TranscriptionRouterService>());
            services.AddScoped<IAudioRecordingService>(sp => sp.GetRequiredService<AudioRecordingRouterService>());

            return services;
        }
    }
}
