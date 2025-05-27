using Meetmind.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Meetmind
{
    public static class MigrationManager
    {
        public static void ApplyMigrations(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<MeetMindDbContext>();
                    Console.WriteLine("Applying migrations...");
                    context.Database.Migrate();

                    var pendingMigrations = context.Database.GetPendingMigrations();

                    if (pendingMigrations.Any())
                    {
                        Console.WriteLine($"Current database migration version: {pendingMigrations.Last()}");
                        Console.WriteLine("Migrations applied successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
        }
    }
}
