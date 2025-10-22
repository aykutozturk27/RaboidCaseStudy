using System.Linq.Expressions;
using MongoDB.Driver;
using RaboidCaseStudy.Application.Abstractions.Repositories;
using RaboidCaseStudy.Domain.Common;

namespace RaboidCaseStudy.Infrastructure.Persistence;
public class GenericRepository<T> : IRepository<T> where T : Entity
{
    private readonly IMongoCollection<T> _colllection;
    public GenericRepository(MongoContext context)
    {
        _colllection = context.GetCollection<T>();
    }

    public async Task<T> InsertAsync(T entity, CancellationToken cancelationToken = default)
    {
        await _colllection.InsertOneAsync(entity, cancellationToken: cancelationToken);
        return entity;
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancelationToken = default)
    {
        return await _colllection.Find(x => x.Id == id).FirstOrDefaultAsync(cancelationToken);
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter, int skip = 0, int take = 100, CancellationToken cancelationToken = default)
    {
        return await _colllection.Find(filter).Skip(skip).Limit(take).ToListAsync(cancelationToken);
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancelationToken = default)
    {
        return await _colllection.CountDocumentsAsync(filter, cancellationToken: cancelationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancelationToken = default)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _colllection.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: cancelationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancelationToken = default)
    {
        await _colllection.DeleteOneAsync(x => x.Id == id, cancelationToken);
    }
}
