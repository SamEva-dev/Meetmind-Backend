using Meetmind.Application.Services;
using Meetmind.Infrastructure.Database;

namespace Meetmind.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MeetMindDbContext _dbContext;

    public UnitOfWork(MeetMindDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
