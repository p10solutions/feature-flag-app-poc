namespace AwsAppConfig.Ecs.Internal;

internal sealed class AgentAppConfigConfigurationSource(
    AppConfigAgentClient agentClient) : IAppConfigConfigurationSource
{
    public async Task<AppConfigFetchResult> FetchAsync(
        AwsAppConfigOptions options,
        CancellationToken cancellationToken)
    {
        var payload = await agentClient.GetConfigurationAsync(options, cancellationToken);
        return new AppConfigFetchResult(payload, options.RefreshIntervalSeconds);
    }
}
