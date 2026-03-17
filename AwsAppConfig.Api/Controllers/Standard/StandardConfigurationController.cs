using AwsAppConfig.Api.Models;
using AwsAppConfig.Ecs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AwsAppConfig.Api.Controllers.Standard;

/// <summary>
/// Endpoints de configuracao e observabilidade para o modo padrao do AWS AppConfig Data.
/// </summary>
[ApiController]
[Route("standard/configuration")]
public sealed class StandardConfigurationController(
    IAppConfigSnapshot snapshot,
    IAppConfigFeatureManager featureManager,
    IOptions<AwsAppConfigOptions> options) : ControllerBase
{
    /// <summary>
    /// Retorna o JSON bruto carregado diretamente do AWS AppConfig Data.
    /// </summary>
    [HttpGet("raw")]
    [ProducesResponseType(typeof(RawAppConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<RawAppConfigResponse> GetRaw()
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        if (string.IsNullOrWhiteSpace(snapshot.RawJson))
        {
            return NotFound(new ApiMessageResponse
            {
                Message = "Nenhuma configuracao recebida ainda do AWS AppConfig.",
                LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
                Mode = options.Value.ConnectionMode.ToString()
            });
        }

        return Ok(new RawAppConfigResponse
        {
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            RawJson = snapshot.RawJson!,
            Mode = "Standard"
        });
    }

    /// <summary>
    /// Retorna as feature flags consolidadas no modo padrao.
    /// </summary>
    [HttpGet("features")]
    [ProducesResponseType(typeof(FeaturesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<FeaturesResponse> GetFeatures()
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        var features = featureManager.GetAll();

        if (features.Count == 0)
        {
            return NotFound(new ApiMessageResponse
            {
                Message = "Configuracao ainda indisponivel ou sem feature flags reconhecidas.",
                LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
                Mode = options.Value.ConnectionMode.ToString()
            });
        }

        return Ok(new FeaturesResponse
        {
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            Features = features,
            Mode = "Standard"
        });
    }

    /// <summary>
    /// Retorna a secao de configuracoes operacionais desserializada do AppConfig.
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(SettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<SettingsResponse> GetSettings()
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        var config = snapshot.Get<AppConfigDemoDocument>();

        if (config is null)
        {
            return NotFound(new ApiMessageResponse
            {
                Message = "Configuracao ainda indisponivel ou fora do formato esperado.",
                LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
                Mode = options.Value.ConnectionMode.ToString()
            });
        }

        return Ok(new SettingsResponse
        {
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            Settings = config.Settings,
            Mode = "Standard"
        });
    }

    private ObjectResult? EnsureMode()
    {
        if (options.Value.ConnectionMode == AppConfigConnectionMode.Direct)
        {
            return null;
        }

        return StatusCode(StatusCodes.Status409Conflict, new ApiMessageResponse
        {
            Message = $"Esta controller representa o modo Standard, mas a API esta configurada para {options.Value.ConnectionMode}.",
            ExpectedMode = "Standard",
            CurrentMode = options.Value.ConnectionMode.ToString()
        });
    }
}
