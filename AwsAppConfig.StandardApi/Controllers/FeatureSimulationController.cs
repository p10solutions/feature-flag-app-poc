using AwsAppConfig.Ecs;
using AwsAppConfig.StandardApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AwsAppConfig.StandardApi.Controllers;

[ApiController]
[Route("simulation")]
public sealed class FeatureSimulationController(
    IAppConfigFeatureManager featureManager,
    IAppConfigSnapshot snapshot) : ControllerBase
{
    [HttpGet("home")]
    public IActionResult GetHome()
    {
        if (featureManager.IsEnabled("maintenance-mode"))
        {
            return Problem(
                title: "Aplicacao temporariamente bloqueada",
                detail: "A flag maintenance-mode foi habilitada no AWS AppConfig.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var config = snapshot.Get<StandardAppConfigDocument>();

        return Ok(new
        {
            Message = config?.Settings.WelcomeMessage ?? "Home liberada.",
            CacheTtlSeconds = config?.Settings.CacheTtlSeconds ?? 60,
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            Mode = "Standard"
        });
    }

    [HttpGet("checkout")]
    public IActionResult GetCheckout()
    {
        if (!featureManager.IsEnabled("checkout-v2"))
        {
            return Problem(
                title: "Checkout V2 bloqueado",
                detail: "Libere a feature checkout-v2 no AWS AppConfig para expor este recurso.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        return Ok(new
        {
            Feature = "checkout-v2",
            Message = "Checkout V2 liberado para consumo.",
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            Mode = "Standard"
        });
    }

    [HttpGet("reports")]
    public IActionResult GetReports()
    {
        if (!featureManager.IsEnabled("reports-v2"))
        {
            return Problem(
                title: "Relatorios V2 bloqueados",
                detail: "A feature reports-v2 esta desligada no AWS AppConfig.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        return Ok(new
        {
            Feature = "reports-v2",
            Message = "Relatorios V2 liberados para este ambiente.",
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc,
            Mode = "Standard"
        });
    }
}
