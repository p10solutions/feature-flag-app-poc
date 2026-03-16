using AwsAppConfig.Ecs.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwsAppConfig.Ecs;

internal sealed class AppConfigPollingHostedService(
    IAppConfigConfigurationSource configurationSource,
    IOptions<AwsAppConfigOptions> options,
    AppConfigState state,
    ILogger<AppConfigPollingHostedService> logger) : BackgroundService
{
    private readonly AwsAppConfigOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextPollIntervalSeconds = _options.RefreshIntervalSeconds;

            try
            {
                var result = await configurationSource.FetchAsync(_options, stoppingToken);
                nextPollIntervalSeconds = result.NextPollIntervalSeconds;

                if (!string.IsNullOrWhiteSpace(result.RawJson))
                {
                    state.Update(result.RawJson, DateTimeOffset.UtcNow);
                }

                logger.LogInformation(
                    "Snapshot do AWS AppConfig atualizado em {UpdatedAtUtc} usando {ConnectionMode}.",
                    state.LastUpdatedAtUtc,
                    _options.ConnectionMode);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Falha ao ler configuracao no AWS AppConfig em modo {ConnectionMode}. Nova tentativa em {Seconds}s.",
                    _options.ConnectionMode,
                    _options.RefreshIntervalSeconds);
            }

            await Task.Delay(
                TimeSpan.FromSeconds(nextPollIntervalSeconds),
                stoppingToken);
        }
    }
}
