using System.Text;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;

namespace AwsAppConfig.Ecs.Internal;

internal sealed class DirectAppConfigConfigurationSource(
    IAmazonAppConfigData appConfigData) : IAppConfigConfigurationSource
{
    private readonly SemaphoreSlim _sessionGate = new(1, 1);
    private string? _configurationToken;

    public async Task<AppConfigFetchResult> FetchAsync(
        AwsAppConfigOptions options,
        CancellationToken cancellationToken)
    {
        await EnsureSessionAsync(options, cancellationToken);

        GetLatestConfigurationResponse response;

        try
        {
            response = await GetLatestConfigurationAsync(cancellationToken);
        }
        catch (BadRequestException)
        {
            _configurationToken = null;
            await EnsureSessionAsync(options, cancellationToken);
            response = await GetLatestConfigurationAsync(cancellationToken);
        }

        _configurationToken = response.NextPollConfigurationToken;

        string? payload = null;

        if (response.Configuration?.Length > 0)
        {
            using var reader = new StreamReader(response.Configuration, Encoding.UTF8);
            payload = await reader.ReadToEndAsync(cancellationToken);
        }

        var nextPoll = response.NextPollIntervalInSeconds > 0
            ? Math.Max(options.RefreshIntervalSeconds, response.NextPollIntervalInSeconds)
            : options.RefreshIntervalSeconds;

        return new AppConfigFetchResult(payload, nextPoll);
    }

    private Task<GetLatestConfigurationResponse> GetLatestConfigurationAsync(
        CancellationToken cancellationToken) =>
        appConfigData.GetLatestConfigurationAsync(
            new GetLatestConfigurationRequest
            {
                ConfigurationToken = _configurationToken
            },
            cancellationToken);

    private async Task EnsureSessionAsync(
        AwsAppConfigOptions options,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_configurationToken))
        {
            return;
        }

        await _sessionGate.WaitAsync(cancellationToken);

        try
        {
            if (!string.IsNullOrWhiteSpace(_configurationToken))
            {
                return;
            }

            var startResponse = await appConfigData.StartConfigurationSessionAsync(
                new StartConfigurationSessionRequest
                {
                    ApplicationIdentifier = options.ApplicationIdentifier,
                    EnvironmentIdentifier = options.EnvironmentIdentifier,
                    ConfigurationProfileIdentifier = options.ConfigurationProfileIdentifier,
                    RequiredMinimumPollIntervalInSeconds = options.RefreshIntervalSeconds
                },
                cancellationToken);

            _configurationToken = startResponse.InitialConfigurationToken;
        }
        finally
        {
            _sessionGate.Release();
        }
    }
}
