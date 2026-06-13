using Habeas.Application.Common;
using Habeas.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Habeas.Application;

/// <summary>Composition of the application layer: use-case handlers and their helpers.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterUser.Command, Guid>, RegisterUser.Handler>();
        services.AddScoped<ICommandHandler<RecordBodyMeasurement.Command, MeasurementView>, RecordBodyMeasurement.Handler>();
        services.AddScoped<IQueryHandler<GetProfile.Query, GetProfile.ProfileView>, GetProfile.Handler>();

        return services;
    }
}
