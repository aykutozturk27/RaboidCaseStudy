using System.Linq.Expressions;
using RaboidCaseStudy.Domain.Common;
namespace RaboidCaseStudy.Application.Abstractions.Repositories;
public interface IRepository<T> where T : Entity
{
    Task<T> InsertAsync(T entity, CancellationToken cancelationToken = default);
    Task<T?> GetByIdAsync(string id, CancellationToken cancelationToken = default);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> filter, int skip = 0, int take = 100, CancellationToken cancelationToken = default);
    Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancelationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancelationToken = default);
    Task DeleteAsync(string id, CancellationToken cancelationToken = default);
}
