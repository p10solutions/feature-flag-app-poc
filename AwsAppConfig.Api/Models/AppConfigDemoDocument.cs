namespace AwsAppConfig.Api.Models;

public sealed class AppConfigDemoDocument
{
    public Dictionary<string, bool> Features { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public AppConfigOperationalSettings Settings { get; init; } = new();
}

public sealed class AppConfigOperationalSettings
{
    public string WelcomeMessage { get; init; } = "AWS AppConfig ativo.";

    public int CacheTtlSeconds { get; init; } = 60;
}
