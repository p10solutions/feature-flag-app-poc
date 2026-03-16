namespace AwsAppConfig.Ecs;

public interface IAppConfigSnapshot
{
    string? RawJson { get; }

    DateTimeOffset? LastUpdatedAtUtc { get; }

    T? Get<T>();

    bool TryGet<T>(out T? value);
}
