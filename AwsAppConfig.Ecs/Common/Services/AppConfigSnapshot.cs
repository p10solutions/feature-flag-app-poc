using AwsAppConfig.Ecs.Common.Internal;

namespace AwsAppConfig.Ecs;

internal sealed class AppConfigSnapshot(AppConfigState state) : IAppConfigSnapshot
{
    public string? RawJson => state.RawJson;

    public DateTimeOffset? LastUpdatedAtUtc => state.LastUpdatedAtUtc;

    public T? Get<T>() => state.TryRead<T>(out var value) ? value : default;

    public bool TryGet<T>(out T? value) => state.TryRead(out value);
}
