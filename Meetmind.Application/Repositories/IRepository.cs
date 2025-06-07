using System.Linq.Expressions;

namespace Meetmind.Application.Repositories;

public interface IRepository<T> where T : class
{
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? filter = null, bool tracking = false);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> filter, bool tracking = false);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);

    Task<PagedResult<T>> ListPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool tracking = false);
}
