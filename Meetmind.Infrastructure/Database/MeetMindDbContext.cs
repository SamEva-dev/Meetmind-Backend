using System.Reflection.Emit;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Database
{
    public class MeetMindDbContext : DbContext
    {
        //public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<SettingsEntity> Settings => Set<SettingsEntity>();
        public DbSet<MeetingEntity> Meetings => Set<MeetingEntity>();

        public DbSet<CalendarSyncLog> CalendarSyncLogs => Set<CalendarSyncLog>();

        public DbSet<MeetingReadModel> MeetingReadModels => Set<MeetingReadModel>();


        //public DbSet<ConsentEntity> Consents => Set<ConsentEntity>();

        public MeetMindDbContext(DbContextOptions<MeetMindDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<Meeting>(e =>
            //{
            //    e.HasKey(x => x.Id);
            //    e.Property(x => x.Title).IsRequired().HasMaxLength(150);
            //    e.Property(x => x.StartUtc).IsRequired();
            //    e.Property(x => x.State).HasConversion<string>();
            //});

            builder.Entity<SettingsEntity>(e =>
            {
                e.HasKey(x => x.Id);
            });
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

                entity.HasIndex(m => new { m.ExternalId, m.ExternalSource })
                      .IsUnique()
                      .HasFilter("[ExternalId] IS NOT NULL AND [ExternalSource] IS NOT NULL");
            });

            builder.Entity<MeetingReadModel>().ToTable("MeetingReadModels");

            builder.Entity<MeetingReadModel>()
                .HasIndex(m => m.StartUtc);

            builder.Entity<MeetingReadModel>()
                .Property(m => m.Title).HasMaxLength(255);
            builder.Entity<MeetingReadModel>()
                    .ToTable("MeetingReadModels")
                    .HasAnnotation("Sqlite:FTS5", "SearchText");


        }
    }
}
