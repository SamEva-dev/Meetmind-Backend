using Meetmind.Domain.Entities;

namespace Meetmind.Application.Common.Interfaces;

public interface IUserSettingsRepository
{
    Task<UserSettingsEntity> GetAsync(Guid userId, CancellationToken ct);
    Task SaveAsync(UserSettingsEntity entity, CancellationToken ct);
}
