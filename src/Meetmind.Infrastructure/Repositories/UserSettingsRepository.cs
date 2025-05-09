using Meetmind.Application.Common.Interfaces;
using Meetmind.Domain.Entities;
using Meetmind.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace Meetmind.Infrastructure.Repositories;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly MeetMindDbContext _db;

    public UserSettingsRepository(MeetMindDbContext db)
    {
        _db = db;
    }

    public async Task<UserSettingsEntity> GetAsync(Guid userId, CancellationToken ct)
    {
        var settings = await _db.UserSettings.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (settings != null) return settings;

        var def = new UserSettingsEntity { Id = userId };
        _db.UserSettings.Add(def);
        await _db.SaveChangesAsync(ct);
        return def;
    }

    public async Task SaveAsync(UserSettingsEntity entity, CancellationToken ct)
    {
        _db.UserSettings.Update(entity);
        await _db.SaveChangesAsync(ct);
    }
}
