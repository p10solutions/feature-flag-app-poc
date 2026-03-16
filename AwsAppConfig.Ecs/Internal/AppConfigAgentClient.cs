using System.Text.Json;

namespace AwsAppConfig.Ecs.Internal;

internal sealed class AppConfigAgentClient(HttpClient httpClient)
{
    public async Task<string> GetConfigurationAsync(
        AwsAppConfigOptions options,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"/applications/{Uri.EscapeDataString(options.ApplicationIdentifier)}" +
            $"/environments/{Uri.EscapeDataString(options.EnvironmentIdentifier)}" +
            $"/configurations/{Uri.EscapeDataString(options.ConfigurationProfileIdentifier)}";

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"AWS AppConfig Agent retornou {(int)response.StatusCode}: {payload}",
                null,
                response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException("AWS AppConfig Agent retornou um payload vazio.");
        }

        using var document = JsonDocument.Parse(payload);
        return document.RootElement.GetRawText();
    }
}
