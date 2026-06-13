using Habeas.Application.Common;
using Habeas.Infrastructure.Messaging;
using Habeas.Infrastructure.Persistence;
using Habeas.Infrastructure.Persistence.Repositories;
using Habeas.Infrastructure.Time;
using Habeas.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Habeas.Infrastructure;

/// <summary>
/// Composition of the infrastructure layer: persistence and supporting services. This is the
/// only place that knows about concrete tech (EF Core, Postgres, the system clock).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var db = configuration.GetSection("Database");
        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = db["Host"],
            Port = int.Parse(db["Port"]!),
            Database = db["Name"],
            Username = db["Username"],
            Password = db["Password"]
        }.ToString();

        services.AddDbContext<HabeasDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
