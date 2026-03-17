using System.Text.Json;
using AwsAppConfig.Ecs.Common.Internal;
using Microsoft.Extensions.Options;

namespace AwsAppConfig.Ecs.Agent;

internal sealed class AgentAppConfigReader(
    AppConfigAgentClient agentClient,
    AppConfigFeatureParser featureParser,
    IOptions<AwsAppConfigOptions> options) : IAppConfigReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AwsAppConfigOptions _options = options.Value;
    private DateTimeOffset? _lastUpdatedAtUtc;

    public DateTimeOffset? LastUpdatedAtUtc => _lastUpdatedAtUtc;

    public async Task<string?> GetRawJsonAsync(CancellationToken cancellationToken = default)
    {
        var payload = await agentClient.GetConfigurationAsync(_options, cancellationToken);
        _lastUpdatedAtUtc = DateTimeOffset.UtcNow;
        return payload;
    }

    public async Task<T?> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        var payload = await GetRawJsonAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(payload))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload, SerializerOptions);
    }

    public async Task<IReadOnlyDictionary<string, bool>> GetFeaturesAsync(CancellationToken cancellationToken = default)
    {
        var payload = await GetRawJsonAsync(cancellationToken);
        return featureParser.Parse(payload);
    }
}
