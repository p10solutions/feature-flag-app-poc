namespace AwsAppConfig.Ecs.Standard;

internal sealed class StandardAppConfigReader(
    IAppConfigSnapshot snapshot,
    IAppConfigFeatureManager featureManager) : IAppConfigReader
{
    public DateTimeOffset? LastUpdatedAtUtc => snapshot.LastUpdatedAtUtc;

    public Task<string?> GetRawJsonAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(snapshot.RawJson);

    public Task<T?> GetAsync<T>(CancellationToken cancellationToken = default) =>
        Task.FromResult(snapshot.Get<T>());

    public Task<IReadOnlyDictionary<string, bool>> GetFeaturesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(featureManager.GetAll());
}
