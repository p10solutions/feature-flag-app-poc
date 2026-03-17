namespace AwsAppConfig.Ecs;

public interface IAppConfigReader
{
    DateTimeOffset? LastUpdatedAtUtc { get; }

    Task<string?> GetRawJsonAsync(CancellationToken cancellationToken = default);

    Task<T?> GetAsync<T>(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, bool>> GetFeaturesAsync(CancellationToken cancellationToken = default);
}
