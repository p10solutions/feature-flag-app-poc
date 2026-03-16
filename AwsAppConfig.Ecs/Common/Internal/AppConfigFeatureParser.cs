using System.Text.Json;

namespace AwsAppConfig.Ecs.Common.Internal;

internal sealed class AppConfigFeatureParser
{
    public IReadOnlyDictionary<string, bool> Parse(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        }

        using var document = JsonDocument.Parse(rawJson);
        var features = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        TryReadSimpleFeatures(document.RootElement, features);
        TryReadAwsHostedFeatures(document.RootElement, features);

        return features;
    }

    private static void TryReadSimpleFeatures(
        JsonElement root,
        IDictionary<string, bool> features)
    {
        if (!root.TryGetProperty("features", out var simpleFeatures) ||
            simpleFeatures.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var item in simpleFeatures.EnumerateObject())
        {
            if (item.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                features[item.Name] = item.Value.GetBoolean();
                continue;
            }

            if (item.Value.ValueKind == JsonValueKind.Object &&
                item.Value.TryGetProperty("enabled", out var enabledProperty) &&
                enabledProperty.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                features[item.Name] = enabledProperty.GetBoolean();
            }
        }
    }

    private static void TryReadAwsHostedFeatures(
        JsonElement root,
        IDictionary<string, bool> features)
    {
        if (!root.TryGetProperty("values", out var awsValues) ||
            awsValues.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var item in awsValues.EnumerateObject())
        {
            if (item.Value.ValueKind == JsonValueKind.Object &&
                item.Value.TryGetProperty("enabled", out var enabledProperty) &&
                enabledProperty.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                features[item.Name] = enabledProperty.GetBoolean();
            }
        }
    }
}
