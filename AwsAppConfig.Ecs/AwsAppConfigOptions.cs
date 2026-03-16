using System.ComponentModel.DataAnnotations;

namespace AwsAppConfig.Ecs;

public sealed class AwsAppConfigOptions
{
    public const string SectionName = "AwsAppConfig";

    public AppConfigConnectionMode ConnectionMode { get; init; } = AppConfigConnectionMode.Agent;

    [Required]
    public string ApplicationIdentifier { get; init; } = string.Empty;

    [Required]
    public string EnvironmentIdentifier { get; init; } = string.Empty;

    [Required]
    public string ConfigurationProfileIdentifier { get; init; } = string.Empty;

    [Url]
    public string? AgentBaseUri { get; init; } = "http://localhost:2772";

    public string? Region { get; init; }

    [Range(300, 86400)]
    public int RefreshIntervalSeconds { get; init; } = 300;
}
