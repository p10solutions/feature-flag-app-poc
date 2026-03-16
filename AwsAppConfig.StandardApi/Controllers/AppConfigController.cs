using AwsAppConfig.Ecs;
using AwsAppConfig.StandardApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AwsAppConfig.StandardApi.Controllers;

[ApiController]
[Route("appconfig")]
public sealed class AppConfigController(
    IAppConfigSnapshot snapshot,
    IAppConfigFeatureManager featureManager) : ControllerBase
{
    [HttpGet("raw")]
    public IActionResult GetRaw()
    {
        if (string.IsNullOrWhiteSpace(snapshot.RawJson))
        {
            return NotFound(new
            {
                Message = "Nenhuma configuracao recebida ainda do AWS AppConfig.",
                snapshot.LastUpdatedAtUtc
            });
        }

        return Ok(new
        {
            snapshot.LastUpdatedAtUtc,
            snapshot.RawJson,
            Mode = "Standard"
        });
    }

    [HttpGet("features")]
    public IActionResult GetFeatures()
    {
        var features = featureManager.GetAll();

        if (features.Count == 0)
        {
            return NotFound(new
            {
                Message = "Configuracao ainda indisponivel ou sem feature flags reconhecidas.",
                snapshot.LastUpdatedAtUtc
            });
        }

        return Ok(new
        {
            snapshot.LastUpdatedAtUtc,
            Features = features,
            Mode = "Standard"
        });
    }

    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        var config = snapshot.Get<StandardAppConfigDocument>();

        if (config is null)
        {
            return NotFound(new
            {
                Message = "Configuracao ainda indisponivel ou fora do formato esperado.",
                snapshot.LastUpdatedAtUtc
            });
        }

        return Ok(new
        {
            snapshot.LastUpdatedAtUtc,
            config.Settings,
            Mode = "Standard"
        });
    }
}
