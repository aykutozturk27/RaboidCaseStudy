using MongoDB.Driver;
using RaboidCaseStudy.Domain.Barcodes;
using RaboidCaseStudy.Domain.Stores;
using RaboidCaseStudy.Domain.Users;
using RaboidCaseStudy.Infrastructure.Persistence;
using RaboidCaseStudy.Infrastructure.Security;

namespace RaboidCaseStudy.Infrastructure.Seed;

public class Seeder
{
    private readonly MongoContext _context;
    public Seeder(MongoContext context) => _context = context;

    public async Task SeedAsync()
    {
        var users = _context.GetCollection<User>();
        var roles = _context.GetCollection<Role>();
        var stores = _context.GetCollection<Store>();
        var ranges = _context.GetCollection<BarcodeRange>();

        // Indexes
        await users.Indexes.CreateOneAsync(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(x => x.Email), new CreateIndexOptions { Unique = true }));
        await roles.Indexes.CreateOneAsync(new CreateIndexModel<Role>(Builders<Role>.IndexKeys.Ascending(x => x.Name), new CreateIndexOptions { Unique = true }));

        // Seed roles
        if (await roles.CountDocumentsAsync(_ => true) == 0)
        {
            await roles.InsertManyAsync(new [] {
                new Role { Name = "Admin" },
                new Role { Name = "Client" }
            });
        }

        // Seed admin user
        if (await users.CountDocumentsAsync(u => u.Email == "admin@raboid.local") == 0)
        {
            var (hash, salt) = PasswordHasher.HashPassword("Admin123!");
            await users.InsertOneAsync(new User {
                Email = "admin@raboid.local",
                PasswordHash = hash,
                Salt = salt,
                Roles = new() { "Admin" }
            });
        }

        // Seed a few stores
        if (await stores.CountDocumentsAsync(_ => true) == 0)
        {
            await stores.InsertManyAsync(new [] {
                new Store { Name = "Central Store" },
                new Store { Name = "Mall Branch" }
            });
        }

        // Seed a barcode range
        if (await ranges.CountDocumentsAsync(_ => true) == 0)
        {
            await ranges.InsertOneAsync(new BarcodeRange { Prefix = "869123", Current = 100000, End = 999999 });
        }
    }
}
