using Jcg.Domain.Core.Repository;
using Jcg.Domain.Core.Tests.Persistence;

namespace Jcg.Domain.Core.Tests.TestCommon;

public class DatabaseAdapter : IDatabaseAdapter
{
    private readonly AppDbContext _db;

    public DatabaseAdapter(
        AppDbContext db)
    {
        _db = db;
    }
    public ICollection<object> GetTrackedEntities()
    {
        return _db.ChangeTracker.Entries().Select(e => e.Entity)
            .Where(e => e != null)
            .Select(e => e!)
            .ToArray();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}