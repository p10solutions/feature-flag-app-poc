namespace AwsAppConfig.Api.Models;

/// <summary>
/// Retorno padrao para cenarios onde a API ainda nao possui uma configuracao carregada
/// ou quando o endpoint solicitado nao corresponde ao modo ativo.
/// </summary>
public sealed class ApiMessageResponse
{
    /// <summary>
    /// Mensagem explicativa para o consumidor da API.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Data da ultima atualizacao conhecida do snapshot em UTC.
    /// </summary>
    public DateTimeOffset? LastUpdatedAtUtc { get; init; }

    /// <summary>
    /// Modo de integracao atualmente configurado na API.
    /// </summary>
    public string? Mode { get; init; }

    /// <summary>
    /// Modo esperado pelo endpoint, quando aplicavel.
    /// </summary>
    public string? ExpectedMode { get; init; }

    /// <summary>
    /// Modo efetivamente configurado na API, quando aplicavel.
    /// </summary>
    public string? CurrentMode { get; init; }
}

/// <summary>
/// Retorno com o JSON bruto lido do AWS AppConfig.
/// </summary>
public sealed class RawAppConfigResponse
{
    /// <summary>
    /// Data da ultima atualizacao do snapshot em UTC.
    /// </summary>
    public DateTimeOffset? LastUpdatedAtUtc { get; init; }

    /// <summary>
    /// JSON bruto retornado pelo AWS AppConfig ou pelo AppConfig Agent.
    /// </summary>
    public string RawJson { get; init; } = string.Empty;

    /// <summary>
    /// Modo de integracao utilizado pela API.
    /// </summary>
    public string Mode { get; init; } = string.Empty;
}

/// <summary>
/// Retorno com as feature flags consolidadas.
/// </summary>
public sealed class FeaturesResponse
{
    /// <summary>
    /// Data da ultima atualizacao do snapshot em UTC.
    /// </summary>
    public DateTimeOffset? LastUpdatedAtUtc { get; init; }

    /// <summary>
    /// Mapa de feature flag para estado ligado ou desligado.
    /// </summary>
    public IReadOnlyDictionary<string, bool> Features { get; init; } =
        new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Modo de integracao utilizado pela API.
    /// </summary>
    public string Mode { get; init; } = string.Empty;
}

/// <summary>
/// Retorno com configuracoes operacionais desserializadas do documento do AppConfig.
/// </summary>
public sealed class SettingsResponse
{
    /// <summary>
    /// Data da ultima atualizacao do snapshot em UTC.
    /// </summary>
    public DateTimeOffset? LastUpdatedAtUtc { get; init; }

    /// <summary>
    /// Configuracoes operacionais lidas do documento de configuracao.
    /// </summary>
    public AppConfigOperationalSettings Settings { get; init; } = new();

    /// <summary>
    /// Modo de integracao utilizado pela API.
    /// </summary>
    public string Mode { get; init; } = string.Empty;
}

/// <summary>
/// Retorno de simulacao da home quando a aplicacao esta liberada.
/// </summary>
public sealed class HomeSimulationResponse
{
    /// <summary>
    /// Mensagem a ser exibida na home.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// TTL de cache recomendado pela configuracao.
    /// </summary>
    public int CacheTtlSeconds { get; init; }

    /// <summary>
    /// Data da ultima atualizacao do snapshot em UTC.
    /// </summary>
    public DateTimeOffset? LastUpdatedAtUtc { get; init; }

    /// <summary>
    /// Modo de integracao utilizado pela API.
    /// </summary>
    public string Mode { get; init; } = string.Empty;
}

/// <summary>
/// Retorno de simulacao para endpoints que liberam uma funcionalidade especifica.
/// </summary>
public sealed class FeatureSimulationResponse
{
    /// <summary>
    /// Nome da funcionalidade validada no AppConfig.
    /// </summary>
    public string Feature { get; init; } = string.Empty;

    /// <summary>
    /// Mensagem funcional retornada pela simulacao.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Data da ultima atualizacao do snapshot em UTC.
    /// </summary>
    public DateTimeOffset? LastUpdatedAtUtc { get; init; }

    /// <summary>
    /// Modo de integracao utilizado pela API.
    /// </summary>
    public string Mode { get; init; } = string.Empty;
}
