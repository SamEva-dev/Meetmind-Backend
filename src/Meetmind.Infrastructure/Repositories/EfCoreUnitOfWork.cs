using Meetmind.Application.Common.Interfaces;
using Meetmind.Infrastructure.Db;

namespace Meetmind.Infrastructure.Repositories;

public class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly MeetMindDbContext _db;
    public EfCoreUnitOfWork(MeetMindDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}