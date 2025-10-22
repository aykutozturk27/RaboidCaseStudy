using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RaboidCaseStudy.Infrastructure.Config;

namespace RaboidCaseStudy.Infrastructure.Persistence;
public class MongoContext
{
    public IMongoDatabase Database { get; }
    public MongoContext(IOptions<MongoSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        Database = client.GetDatabase(options.Value.Database);
    }
    public IMongoCollection<T> GetCollection<T>(string? name = null) =>
        Database.GetCollection<T>(name ?? typeof(T).Name);
}
