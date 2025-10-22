using System.Linq.Expressions;
using MongoDB.Driver;
using RaboidCaseStudy.Application.Abstractions.Repositories;
using RaboidCaseStudy.Domain.Common;

namespace RaboidCaseStudy.Infrastructure.Persistence;
public class GenericRepository<T> : IRepository<T> where T : Entity
{
    private readonly IMongoCollection<T> _col;
    public GenericRepository(MongoContext ctx)
    {
        _col = ctx.GetCollection<T>();
    }
    public async Task<T> InsertAsync(T entity, CancellationToken ct = default)
    {
        await _col.InsertOneAsync(entity, cancellationToken: ct);
        return entity;
    }
    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }
    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        return await _col.Find(filter).Skip(skip).Limit(take).ToListAsync(ct);
    }
    public async Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default)
    {
        return await _col.CountDocumentsAsync(filter, cancellationToken: ct);
    }
    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _col.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await _col.DeleteOneAsync(x => x.Id == id, ct);
    }
}
