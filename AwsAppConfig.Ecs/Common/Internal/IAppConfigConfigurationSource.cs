namespace AwsAppConfig.Ecs.Common.Internal;

internal interface IAppConfigConfigurationSource
{
    Task<AppConfigFetchResult> FetchAsync(
        AwsAppConfigOptions options,
        CancellationToken cancellationToken);
}
