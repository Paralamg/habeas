using Habeas.Bot.Telegram;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Habeas.Bot;

internal static class DependencyInjection
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TelegramOptions>()
            .Bind(configuration.GetSection(TelegramOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ITelegramBotClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<TelegramOptions>>().Value;
            return new TelegramBotClient(options.BotToken);
        });

        services.AddSingleton<HabeasUpdateHandler>();
        services.AddScoped<BotCommandRouter>();
        services.AddHostedService<BotPollingService>();

        return services;
    }
}
