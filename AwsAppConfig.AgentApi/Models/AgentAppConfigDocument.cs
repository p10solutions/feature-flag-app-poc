namespace AwsAppConfig.AgentApi.Models;

public sealed class AgentAppConfigDocument
{
    public Dictionary<string, bool> Features { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public AgentOperationalSettings Settings { get; init; } = new();
}

public sealed class AgentOperationalSettings
{
    public string WelcomeMessage { get; init; } = "AppConfig Agent ativo.";

    public int CacheTtlSeconds { get; init; } = 60;
}
