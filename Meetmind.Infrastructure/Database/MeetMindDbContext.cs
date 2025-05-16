

using Meetmind.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Database
{
    public class MeetMindDbContext : DbContext
    {
        //public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<SettingsEntity> Settings => Set<SettingsEntity>();
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
        }
    }
}
