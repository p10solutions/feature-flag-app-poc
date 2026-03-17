using AwsAppConfig.Api.Models;
using AwsAppConfig.Ecs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AwsAppConfig.Api.Controllers.Agent;

/// <summary>
/// Endpoints de negocio protegidos por feature flags no modo Agent.
/// </summary>
[ApiController]
[Route("agent/business")]
public sealed class AgentBusinessController(
    IAppConfigReader reader,
    IOptions<AwsAppConfigOptions> options) : ControllerBase
{
    /// <summary>
    /// Simula o acesso a home da aplicacao.
    /// </summary>
    [HttpGet("home")]
    [ProducesResponseType(typeof(HomeSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HomeSimulationResponse>> GetHome(CancellationToken cancellationToken)
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        var features = await reader.GetFeaturesAsync(cancellationToken);

        if (features.TryGetValue("maintenance-mode", out var maintenanceMode) && maintenanceMode)
        {
            return Problem(
                title: "Aplicacao temporariamente bloqueada",
                detail: "A flag maintenance-mode foi habilitada no AWS AppConfig.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var config = await reader.GetAsync<AppConfigDemoDocument>(cancellationToken);

        return Ok(new HomeSimulationResponse
        {
            Message = config?.Settings.WelcomeMessage ?? "Home liberada.",
            CacheTtlSeconds = config?.Settings.CacheTtlSeconds ?? 60,
            LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
            Mode = "Agent"
        });
    }

    /// <summary>
    /// Simula o checkout condicionado pela flag <c>checkout-v2</c>.
    /// </summary>
    [HttpGet("checkout")]
    [ProducesResponseType(typeof(FeatureSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeatureSimulationResponse>> GetCheckout(CancellationToken cancellationToken)
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        var features = await reader.GetFeaturesAsync(cancellationToken);

        if (!features.TryGetValue("checkout-v2", out var checkoutEnabled) || !checkoutEnabled)
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
            LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
            Mode = "Agent"
        });
    }

    /// <summary>
    /// Simula o acesso aos relatorios condicionado pela flag <c>reports-v2</c>.
    /// </summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(FeatureSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeatureSimulationResponse>> GetReports(CancellationToken cancellationToken)
    {
        var modeCheck = EnsureMode();
        if (modeCheck is not null)
        {
            return modeCheck;
        }

        var features = await reader.GetFeaturesAsync(cancellationToken);

        if (!features.TryGetValue("reports-v2", out var reportsEnabled) || !reportsEnabled)
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
            LastUpdatedAtUtc = reader.LastUpdatedAtUtc,
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
