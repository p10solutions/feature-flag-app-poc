using AwsAppConfig.Api.Models;
using AwsAppConfig.Ecs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AwsAppConfig.Api.Controllers;

/// <summary>
/// Endpoints de demonstracao para o modo padrao do AWS AppConfig Data.
/// Use esta controller quando a API estiver configurada com <c>ConnectionMode=Direct</c>.
/// </summary>
[ApiController]
[Route("standard")]
public sealed class StandardController(
    IAppConfigSnapshot snapshot,
    IAppConfigFeatureManager featureManager,
    IOptions<AwsAppConfigOptions> options) : ControllerBase
{
    /// <summary>
    /// Retorna o documento bruto carregado diretamente do AWS AppConfig Data.
    /// </summary>
    [HttpGet("appconfig/raw")]
    [ProducesResponseType(typeof(RawAppConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<RawAppConfigResponse> GetRaw()
    {
        var modeCheck = EnsureMode(AppConfigConnectionMode.Direct);
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
    [HttpGet("appconfig/features")]
    [ProducesResponseType(typeof(FeaturesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<FeaturesResponse> GetFeatures()
    {
        var modeCheck = EnsureMode(AppConfigConnectionMode.Direct);
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
    /// Retorna a secao de configuracoes operacionais desserializada do documento AppConfig.
    /// </summary>
    [HttpGet("appconfig/settings")]
    [ProducesResponseType(typeof(SettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<SettingsResponse> GetSettings()
    {
        var modeCheck = EnsureMode(AppConfigConnectionMode.Direct);
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

    /// <summary>
    /// Simula o acesso a home da aplicacao no modo padrao.
    /// </summary>
    [HttpGet("simulation/home")]
    [ProducesResponseType(typeof(HomeSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<HomeSimulationResponse> GetHome()
    {
        var modeCheck = EnsureMode(AppConfigConnectionMode.Direct);
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        if (featureManager.IsEnabled("maintenance-mode"))
        {
            return Problem(
                title: "Aplicacao temporariamente bloqueada",
                detail: "A flag maintenance-mode foi habilitada no AWS AppConfig.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var config = snapshot.Get<AppConfigDemoDocument>();

        return Ok(new HomeSimulationResponse
        {
            Message = config?.Settings.WelcomeMessage ?? "Home liberada.",
            CacheTtlSeconds = config?.Settings.CacheTtlSeconds ?? 60,
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            Mode = "Standard"
        });
    }

    /// <summary>
    /// Simula o acesso ao checkout condicionado pela flag <c>checkout-v2</c>.
    /// </summary>
    [HttpGet("simulation/checkout")]
    [ProducesResponseType(typeof(FeatureSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<FeatureSimulationResponse> GetCheckout()
    {
        var modeCheck = EnsureMode(AppConfigConnectionMode.Direct);
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        if (!featureManager.IsEnabled("checkout-v2"))
        {
            return Problem(
                title: "Checkout V2 bloqueado",
                detail: "Libere a feature checkout-v2 no AWS AppConfig para expor este recurso.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        return Ok(new FeatureSimulationResponse
        {
            Feature = "checkout-v2",
            Message = "Checkout V2 liberado para consumo.",
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            Mode = "Standard"
        });
    }

    /// <summary>
    /// Simula o acesso aos relatorios condicionado pela flag <c>reports-v2</c>.
    /// </summary>
    [HttpGet("simulation/reports")]
    [ProducesResponseType(typeof(FeatureSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public ActionResult<FeatureSimulationResponse> GetReports()
    {
        var modeCheck = EnsureMode(AppConfigConnectionMode.Direct);
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        if (!featureManager.IsEnabled("reports-v2"))
        {
            return Problem(
                title: "Relatorios V2 bloqueados",
                detail: "A feature reports-v2 esta desligada no AWS AppConfig.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        return Ok(new FeatureSimulationResponse
        {
            Feature = "reports-v2",
            Message = "Relatorios V2 liberados para este ambiente.",
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            Mode = "Standard"
        });
    }

    private ObjectResult? EnsureMode(AppConfigConnectionMode expectedMode)
    {
        if (options.Value.ConnectionMode == expectedMode)
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
