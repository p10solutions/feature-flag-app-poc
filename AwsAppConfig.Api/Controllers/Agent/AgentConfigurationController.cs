using AwsAppConfig.Api.Models;
using AwsAppConfig.Ecs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AwsAppConfig.Api.Controllers.Agent;

/// <summary>
/// Endpoints de configuracao e observabilidade para o modo com AWS AppConfig Agent.
/// </summary>
[ApiController]
[Route("agent/configuration")]
public sealed class AgentConfigurationController(
    IAppConfigReader reader,
    IOptions<AwsAppConfigOptions> options) : ControllerBase
{
    /// <summary>
    /// Retorna o JSON bruto carregado pelo AppConfig Agent.
    /// </summary>
    [HttpGet("raw")]
    [ProducesResponseType(typeof(RawAppConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RawAppConfigResponse>> GetRaw(CancellationToken cancellationToken)
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        var rawJson = await reader.GetRawJsonAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return NotFound(new ApiMessageResponse
            {
                Message = "Nenhuma configuracao recebida ainda do AWS AppConfig.",
                LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
                Mode = options.Value.ConnectionMode.ToString()
            });
        }

        return Ok(new RawAppConfigResponse
        {
            LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
            RawJson = rawJson,
            Mode = "Agent"
        });
    }

    /// <summary>
    /// Retorna as feature flags consolidadas do documento carregado pelo Agent.
    /// </summary>
    [HttpGet("features")]
    [ProducesResponseType(typeof(FeaturesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeaturesResponse>> GetFeatures(CancellationToken cancellationToken)
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        var features = await reader.GetFeaturesAsync(cancellationToken);

        if (features.Count == 0)
        {
            return NotFound(new ApiMessageResponse
            {
                Message = "Configuracao ainda indisponivel ou sem feature flags reconhecidas.",
                LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
                Mode = options.Value.ConnectionMode.ToString()
            });
        }

        return Ok(new FeaturesResponse
        {
            LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
            Features = features,
            Mode = "Agent"
        });
    }

    /// <summary>
    /// Retorna a secao de configuracoes operacionais desserializada do AppConfig.
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(SettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SettingsResponse>> GetSettings(CancellationToken cancellationToken)
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        var config = await reader.GetAsync<AppConfigDemoDocument>(cancellationToken);

        if (config is null)
        {
            return NotFound(new ApiMessageResponse
            {
                Message = "Configuracao ainda indisponivel ou fora do formato esperado.",
                LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
                Mode = options.Value.ConnectionMode.ToString()
            });
        }

        return Ok(new SettingsResponse
        {
            LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
            Settings = config.Settings,
            Mode = "Agent"
        });
    }

    private ObjectResult? EnsureMode()
    {
        if (options.Value.ConnectionMode == AppConfigConnectionMode.Agent)
        {
            return null;
        }

        return StatusCode(StatusCodes.Status409Conflict, new ApiMessageResponse
        {
            Message = $"Esta controller representa o modo Agent, mas a API esta configurada para {options.Value.ConnectionMode}.",
            ExpectedMode = "Agent",
            CurrentMode = options.Value.ConnectionMode.ToString()
        });
    }
}
