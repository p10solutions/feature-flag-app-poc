namespace AwsAppConfig.Ecs.Internal;

internal sealed record AppConfigFetchResult(string? RawJson, int NextPollIntervalSeconds);
