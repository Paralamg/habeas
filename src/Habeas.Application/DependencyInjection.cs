using Habeas.Application.Common;
using Habeas.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Habeas.Application;

/// <summary>Composition of the application layer: use-case handlers and their helpers.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterUser.Command, RegisterUser.Registration>, RegisterUser.Handler>();
        services.AddScoped<ICommandHandler<SetDateOfBirth.Command, SetDateOfBirth.DateOfBirthView>, SetDateOfBirth.Handler>();
        services.AddScoped<ICommandHandler<RecordBodyMeasurement.Command, MeasurementView>, RecordBodyMeasurement.Handler>();
        services.AddScoped<IQueryHandler<IsRegistered.Query, bool>, IsRegistered.Handler>();
        services.AddScoped<IQueryHandler<GetProfile.Query, GetProfile.ProfileView>, GetProfile.Handler>();
        services.AddScoped<IQueryHandler<GetMeasurementHistory.Query, GetMeasurementHistory.HistoryView>, GetMeasurementHistory.Handler>();

        return services;
    }
}
