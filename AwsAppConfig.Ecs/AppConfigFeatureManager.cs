using AwsAppConfig.Ecs.Internal;

namespace AwsAppConfig.Ecs;

internal sealed class AppConfigFeatureManager(
    IAppConfigSnapshot snapshot,
    AppConfigFeatureParser featureParser) : IAppConfigFeatureManager
{
    public IReadOnlyDictionary<string, bool> GetAll() => featureParser.Parse(snapshot.RawJson);

    public bool IsEnabled(string featureName) =>
        TryGet(featureName, out var enabled) && enabled;

    public bool TryGet(string featureName, out bool enabled)
    {
        var features = GetAll();
        return features.TryGetValue(featureName, out enabled);
    }
}
