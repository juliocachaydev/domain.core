using System.Reflection;
using Jcg.Domain.Core.Microsoft.DependencyInjection;
using Jcg.Domain.Core.Tests.Persistence;
using Jcg.Domain.Core.Tests.TestCommon;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jcg.Domain.Core.Tests;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Ef Core
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connection));
        
        services.AddRepository(
            sp => new DatabaseAdapter(sp.GetRequiredService<AppDbContext>()),
            Assembly.GetExecutingAssembly());
    }
}