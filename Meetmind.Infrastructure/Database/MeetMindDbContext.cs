using System.Reflection.Emit;
using System.Text.Json;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Models;
using Meetmind.Domain.Units;
using Meetmind.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Database
{
    public class MeetMindDbContext : DbContext
    {
        private readonly IDomainEventDispatcher? _dispatcher;

        public MeetMindDbContext(DbContextOptions<MeetMindDbContext> options,
                                 IDomainEventDispatcher? dispatcher = null)
            : base(options)
        {
            _dispatcher = dispatcher;
        }

        public DbSet<SettingsEntity> Settings => Set<SettingsEntity>();
        public DbSet<MeetingEntity> Meetings => Set<MeetingEntity>();
        public DbSet<TranscriptionEntity> Transcriptions => Set<TranscriptionEntity>();
        public DbSet<TranscriptionSegment> Segments => Set<TranscriptionSegment>();
        public DbSet<CalendarSyncLog> CalendarSyncLogs => Set<CalendarSyncLog>();
        public DbSet<AudioEventLog> AudioEventLogs { get; set; }

        public DbSet<MeetingReadModel> MeetingReadModels => Set<MeetingReadModel>();

        public DbSet<AudioMetadata> AudioMetadatas { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_dispatcher != null)
            {
                var domainEntities = ChangeTracker
                    .Entries<AggregateRoot>()
                    .Where(e => e.Entity.DomainEvents.Any())
                    .Select(e => e.Entity)
                    .ToList();

                await _dispatcher.DispatchEventsAsync(domainEntities);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // SettingsEntity mapping
            builder.Entity<SettingsEntity>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.TranscriptionType)
                    .HasConversion<string>();

                e.Property(x => x.AudioRecordingType)
                    .HasConversion<string>();

                e.Property(x => x.WhisperDeviceType)
                    .HasConversion<string>();

                e.Property(x => x.WhisperComputeType)
                    .HasConversion<string>();

                e.Property(x => x.WhisperModelType)
                    .HasConversion<string>();

                e.Property(x => x.DiarizationModelType)
                    .HasConversion<string>();
            });

            // MeetingEntity mapping
            builder.Entity<MeetingEntity>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(m => m.StartUtc).IsRequired();

                entity.Property(m => m.ExternalId)
                      .HasMaxLength(256);

                entity.Property(m => m.ExternalSource)
                      .HasMaxLength(50);

                // SQLite ne gère pas HasFilter (WHERE) sur index
                entity.HasIndex(m => new { m.ExternalId, m.ExternalSource })
                      .IsUnique();
            });

            // MeetingReadModel mapping
            builder.Entity<MeetingReadModel>(e =>
            {
                e.ToTable("MeetingReadModels");
                e.HasKey(m => m.Id);
                e.Property(m => m.Title).HasMaxLength(255);
                e.HasIndex(m => m.StartUtc);
                // FTS5 non supporté via EF, ignore pour le moment ou configure à la main si besoin
            });

            // TranscriptionEntity mapping
            builder.Entity<TranscriptionEntity>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(t => t.Tilte)
                      .HasMaxLength(255);

                // 1 Transcription -> n Segments
                entity.HasMany(t => t.Segments)
                      .WithOne(s => s.Transcription)
                      .HasForeignKey(s => s.TranscriptionId)
                      .OnDelete(DeleteBehavior.Cascade); // Important pour la suppression en cascade

                // Optionnel: stocker Speakers comme JSON si besoin
                entity.Property(t => t.Speakers)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                          v => string.IsNullOrEmpty(v) ? new List<string>() : 
                                JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
                      );
            });

            // TranscriptionSegment mapping
            builder.Entity<TranscriptionSegment>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Speaker).HasMaxLength(100);
                entity.Property(s => s.Text).IsRequired();
                entity.Property(s => s.Start).HasMaxLength(20);
                entity.Property(s => s.End).HasMaxLength(20);
            });

            // CalendarSyncLog mapping
            builder.Entity<CalendarSyncLog>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Source).HasMaxLength(100);
            });
        }
    }

}
