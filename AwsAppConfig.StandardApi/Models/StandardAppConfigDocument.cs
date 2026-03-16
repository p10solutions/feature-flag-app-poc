namespace AwsAppConfig.StandardApi.Models;

public sealed class StandardAppConfigDocument
{
    public Dictionary<string, bool> Features { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public StandardOperationalSettings Settings { get; init; } = new();
}

public sealed class StandardOperationalSettings
{
    public string WelcomeMessage { get; init; } = "Modo padrao do AWS AppConfig ativo.";

    public int CacheTtlSeconds { get; init; } = 60;
}
