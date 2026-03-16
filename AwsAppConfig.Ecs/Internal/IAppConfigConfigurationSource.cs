namespace AwsAppConfig.Ecs.Internal;

internal interface IAppConfigConfigurationSource
{
    Task<AppConfigFetchResult> FetchAsync(
        AwsAppConfigOptions options,
        CancellationToken cancellationToken);
}
