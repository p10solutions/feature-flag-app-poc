namespace AwsAppConfig.Ecs;

public interface IAppConfigFeatureManager
{
    IReadOnlyDictionary<string, bool> GetAll();

    bool IsEnabled(string featureName);

    bool TryGet(string featureName, out bool enabled);
}
