namespace AwsAppConfig.Ecs.Common.Internal;

internal sealed record AppConfigFetchResult(string? RawJson, int NextPollIntervalSeconds);
