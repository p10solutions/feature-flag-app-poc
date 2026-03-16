using System.Text.Json;

namespace AwsAppConfig.Ecs.Common.Internal;

internal sealed class AppConfigState
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly object _gate = new();
    private string? _rawJson;
    private DateTimeOffset? _lastUpdatedAtUtc;

    public string? RawJson
    {
        get
        {
            lock (_gate)
            {
                return _rawJson;
            }
        }
    }

    public DateTimeOffset? LastUpdatedAtUtc
    {
        get
        {
            lock (_gate)
            {
                return _lastUpdatedAtUtc;
            }
        }
    }

    public void Update(string rawJson, DateTimeOffset updatedAtUtc)
    {
        lock (_gate)
        {
            _rawJson = rawJson;
            _lastUpdatedAtUtc = updatedAtUtc;
        }
    }

    public bool TryRead<T>(out T? value)
    {
        var snapshot = RawJson;

        if (string.IsNullOrWhiteSpace(snapshot))
        {
            value = default;
            return false;
        }

        value = JsonSerializer.Deserialize<T>(snapshot, SerializerOptions);

        return value is not null;
    }
}
