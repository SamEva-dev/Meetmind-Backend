using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meetmind.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Db;

public class MeetMindDbContext : DbContext
{
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<UserSettingsEntity> UserSettings => Set<UserSettingsEntity>();


    public MeetMindDbContext(DbContextOptions<MeetMindDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Meeting>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(150);
            e.Property(x => x.StartUtc).IsRequired();
            e.Property(x => x.State).HasConversion<string>();
        });
    }
}