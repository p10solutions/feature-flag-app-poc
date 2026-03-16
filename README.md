# PoC .NET + AWS AppConfig

Esta PoC tem dois projetos ativos na solucao:

- `AwsAppConfig.Ecs`: biblioteca reutilizavel para consumo de configuracao e feature flags do AWS AppConfig.
- `AwsAppConfig.Api`: API unica de demonstracao.

## Sobre a quantidade de classes

Nao, nao era obrigatorio ter tantas classes para uma PoC.

O minimo para funcionar seria bem menor. Eu separei mais responsabilidades para manter a lib flexivel entre `Agent` e `Direct`, mas para a API de exemplo isso acabou deixando a leitura menos obvia do que deveria.

Para resolver isso, a API agora foi organizada por modo de uso:

- `AgentController`: representa explicitamente o fluxo com `AWS AppConfig Agent`.
- `StandardController`: representa explicitamente o fluxo no modo padrao, direto no `AWS AppConfig Data`.

Assim voce consegue testar e entender cada integracao olhando apenas a controller correspondente.

Na biblioteca `AwsAppConfig.Ecs`, a organizacao tambem foi separada por responsabilidade:

- `Agent/`: classes exclusivas do modo com AppConfig Agent.
- `Standard/`: classes exclusivas do modo padrao, direto no AppConfig Data.
- `Common/`: classes compartilhadas pelos dois modos, como parsing, estado em memoria e servicos compartilhados.
- `Abstractions/`: contratos publicos expostos pela lib.
- `Configuration/`: tipos de configuracao publica da lib.
- `Hosting/`: classes de execucao em background.
- `Composition/`: registro de DI e composicao da biblioteca.

## Como a API funciona

A API unica usa a biblioteca `AwsAppConfig.Ecs`, mas agora expõe rotas separadas por modo:

### Rotas do modo Agent

- `GET /agent/appconfig/raw`
- `GET /agent/appconfig/features`
- `GET /agent/appconfig/settings`
- `GET /agent/simulation/home`
- `GET /agent/simulation/checkout`
- `GET /agent/simulation/reports`

### Rotas do modo Standard

- `GET /standard/appconfig/raw`
- `GET /standard/appconfig/features`
- `GET /standard/appconfig/settings`
- `GET /standard/simulation/home`
- `GET /standard/simulation/checkout`
- `GET /standard/simulation/reports`

## Regra de uso

A configuracao continua sendo controlada por `AwsAppConfig:ConnectionMode`.

- Se a API estiver em `Agent`, as rotas `/agent/...` funcionam e as `/standard/...` retornam `409` explicando o desencontro.
- Se a API estiver em `Direct`, as rotas `/standard/...` funcionam e as `/agent/...` retornam `409`.

Isso deixa visivel qual controller representa qual estrategia, sem precisar manter duas APIs separadas.

## Configuracao base

Arquivo: `AwsAppConfig.Api/appsettings.json`

```json
"AwsAppConfig": {
  "ConnectionMode": "Agent",
  "ApplicationIdentifier": "sample-checkout",
  "EnvironmentIdentifier": "ecs-dev",
  "ConfigurationProfileIdentifier": "feature-flags",
  "AgentBaseUri": "http://localhost:2772",
  "Region": "us-east-1",
  "RefreshIntervalSeconds": 300
}
```

Para usar o modo padrao sem agent:

```json
"ConnectionMode": "Direct"
```

## Registro na aplicacao

```csharp
using AwsAppConfig.Ecs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAwsAppConfigEcs(builder.Configuration);
```

## Consumo em servicos

```csharp
using AwsAppConfig.Ecs;

public sealed class CheckoutService(
    IAppConfigSnapshot snapshot,
    IAppConfigFeatureManager featureManager)
{
    public bool PodeUsarCheckoutV2() => featureManager.IsEnabled("checkout-v2");

    public T? LerConfiguracao<T>() => snapshot.Get<T>();
}
```

## Configuracao para ECS com AppConfig Agent

```json
{
  "family": "sample-api-task",
  "networkMode": "awsvpc",
  "containerDefinitions": [
    {
      "name": "aws-appconfig-agent",
      "image": "public.ecr.aws/aws-appconfig/aws-appconfig-agent:2.x",
      "essential": true,
      "environment": [
        {
          "name": "SERVICE_REGION",
          "value": "us-east-1"
        },
        {
          "name": "POLL_INTERVAL",
          "value": "5m"
        },
        {
          "name": "PREFETCH_LIST",
          "value": "/applications/sample-checkout/environments/ecs-dev/configurations/feature-flags"
        }
      ]
    },
    {
      "name": "sample-api",
      "image": "<sua-imagem-api>",
      "essential": true,
      "environment": [
        {
          "name": "AwsAppConfig__ConnectionMode",
          "value": "Agent"
        },
        {
          "name": "AwsAppConfig__ApplicationIdentifier",
          "value": "sample-checkout"
        },
        {
          "name": "AwsAppConfig__EnvironmentIdentifier",
          "value": "ecs-dev"
        },
        {
          "name": "AwsAppConfig__ConfigurationProfileIdentifier",
          "value": "feature-flags"
        },
        {
          "name": "AwsAppConfig__AgentBaseUri",
          "value": "http://localhost:2772"
        },
        {
          "name": "AwsAppConfig__Region",
          "value": "us-east-1"
        },
        {
          "name": "AwsAppConfig__RefreshIntervalSeconds",
          "value": "300"
        }
      ]
    }
  ]
}
```

## Configuracao para o modo padrao sem agent

Use as mesmas variaveis, mudando apenas `ConnectionMode` para `Direct`.

## IAM minimo

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "appconfig:StartConfigurationSession",
        "appconfig:GetLatestConfiguration"
      ],
      "Resource": "*"
    }
  ]
}
```

## Exemplo de payload no AppConfig

```json
{
  "features": {
    "checkout-v2": true,
    "reports-v2": false,
    "maintenance-mode": false
  },
  "settings": {
    "welcomeMessage": "Configuracao entregue pelo AWS AppConfig.",
    "cacheTtlSeconds": 45
  }
}
```

## Executar

```powershell
dotnet build AwsAppConfig.Poc.sln -c Release
dotnet run --project .\AwsAppConfig.Api\AwsAppConfig.Api.csproj
```

## Referencias oficiais AWS

- AWS AppConfig Agent para containers: https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-containers-agent.html
- Variaveis do agent (`POLL_INTERVAL`, `PREFETCH_LIST`, `SERVICE_REGION`): https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-containers-agent-configuring.html
- Uso local do agent em `localhost:2772`: https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-agent-how-to-use-local-development-samples.html
- AppConfig Data API (`StartConfigurationSession`, `GetLatestConfiguration`): https://docs.aws.amazon.com/appconfig/2019-10-09/APIReference/Welcome.html
