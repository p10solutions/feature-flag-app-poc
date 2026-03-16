using Amazon;
using Amazon.AppConfigData;
using AwsAppConfig.Ecs.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AwsAppConfig.Ecs;

public static class DependencyInjection
{
    public static IServiceCollection AddAwsAppConfigEcs(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = AwsAppConfigOptions.SectionName)
    {
        services
            .AddOptions<AwsAppConfigOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .Validate(ValidateOptions, BuildValidationMessage())
            .ValidateOnStart();

        services.AddSingleton<AppConfigState>();
        services.AddSingleton<AppConfigFeatureParser>();
        services.AddSingleton<IAppConfigSnapshot, AppConfigSnapshot>();
        services.AddSingleton<IAppConfigFeatureManager, AppConfigFeatureManager>();
        services.AddSingleton<AgentAppConfigConfigurationSource>();
        services.AddSingleton<DirectAppConfigConfigurationSource>();
        services.AddSingleton<IAppConfigConfigurationSource>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AwsAppConfigOptions>>().Value;

            return options.ConnectionMode switch
            {
                AppConfigConnectionMode.Agent => sp.GetRequiredService<AgentAppConfigConfigurationSource>(),
                AppConfigConnectionMode.Direct => sp.GetRequiredService<DirectAppConfigConfigurationSource>(),
                _ => throw new InvalidOperationException(
                    $"Modo de conexao AppConfig nao suportado: {options.ConnectionMode}.")
            };
        });

        services.AddHttpClient<AppConfigAgentClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AwsAppConfigOptions>>().Value;
            client.BaseAddress = new Uri(options.AgentBaseUri!);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddSingleton<IAmazonAppConfigData>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AwsAppConfigOptions>>().Value;
            return string.IsNullOrWhiteSpace(options.Region)
                ? new AmazonAppConfigDataClient()
                : new AmazonAppConfigDataClient(RegionEndpoint.GetBySystemName(options.Region));
        });

        services.AddHostedService<AppConfigPollingHostedService>();

        return services;
    }

    private static bool ValidateOptions(AwsAppConfigOptions options)
    {
        return options.ConnectionMode switch
        {
            AppConfigConnectionMode.Agent => Uri.TryCreate(options.AgentBaseUri, UriKind.Absolute, out _),
            AppConfigConnectionMode.Direct => true,
            _ => false
        };
    }

    private static string BuildValidationMessage() =>
        "AwsAppConfig invalido. Para Agent, informe AgentBaseUri absoluta. Para Direct, configure ApplicationIdentifier, EnvironmentIdentifier, ConfigurationProfileIdentifier e, opcionalmente, Region.";
}
