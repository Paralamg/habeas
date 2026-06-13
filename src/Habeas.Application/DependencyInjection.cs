using Habeas.Application.Common;
using Habeas.Application.Users;
using Habeas.Application.Users.GetProfile;
using Habeas.Application.Users.RegisterUser;
using Habeas.Application.Users.SetBodyMetrics;
using Microsoft.Extensions.DependencyInjection;

namespace Habeas.Application;

/// <summary>Composition of the application layer: use-case handlers and their helpers.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterUserCommand, Guid>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<SetBodyMetricsCommand, BodyMetricsView>, SetBodyMetricsCommandHandler>();
        services.AddScoped<IQueryHandler<GetProfileQuery, ProfileView>, GetProfileQueryHandler>();

        return services;
    }
}
