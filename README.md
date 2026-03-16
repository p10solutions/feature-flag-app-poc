# PoC .NET + AWS AppConfig para ECS e acesso direto

Esta PoC entrega tres projetos:

- `AwsAppConfig.Ecs`: biblioteca reutilizavel para consumo de configuracao e feature flags do AWS AppConfig.
- `AwsAppConfig.AgentApi`: API de exemplo usando `AWS AppConfig Agent` como sidecar no ECS.
- `AwsAppConfig.StandardApi`: API de exemplo usando o modo padrao do AWS AppConfig Data, sem agent.

## O que foi melhorado na arquitetura

A biblioteca foi refatorada para ficar mais flexivel e aderente a boas praticas:

- `AppConfigPollingHostedService` depende de uma abstracao (`IAppConfigConfigurationSource`) em vez de depender do agent diretamente.
- O parsing de feature flags saiu do estado em memoria e foi isolado em `AppConfigFeatureParser`.
- A configuracao passou a explicitar o modo de conexao com `ConnectionMode = Agent | Direct`.
- A biblioteca continua expondo contratos simples para as aplicacoes consumidoras: `IAppConfigSnapshot` e `IAppConfigFeatureManager`.

Isso melhora SRP, OCP e reduz acoplamento de infraestrutura.

## Biblioteca reutilizavel

Registro padrao em qualquer aplicacao:

```csharp
using AwsAppConfig.Ecs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAwsAppConfigEcs(builder.Configuration);
```

Consumo:

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

## Modo 1: uso com AWS AppConfig Agent

Configuracao da API que roda no ECS com sidecar:

```json
"AwsAppConfig": {
  "ConnectionMode": "Agent",
  "ApplicationIdentifier": "sample-checkout",
  "EnvironmentIdentifier": "ecs-dev",
  "ConfigurationProfileIdentifier": "feature-flags",
  "AgentBaseUri": "http://localhost:2772",
  "RefreshIntervalSeconds": 300
}
```

### Task Definition de exemplo

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
      "dependsOn": [
        {
          "containerName": "aws-appconfig-agent",
          "condition": "START"
        }
      ],
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
          "name": "AwsAppConfig__RefreshIntervalSeconds",
          "value": "300"
        }
      ]
    }
  ]
}
```

## Modo 2: uso direto sem agent

Configuracao da API que consulta diretamente o AppConfigData:

```json
"AwsAppConfig": {
  "ConnectionMode": "Direct",
  "ApplicationIdentifier": "sample-checkout",
  "EnvironmentIdentifier": "ecs-dev",
  "ConfigurationProfileIdentifier": "feature-flags",
  "Region": "us-east-1",
  "RefreshIntervalSeconds": 300
}
```

Nesse modo, a biblioteca abre a sessao via `StartConfigurationSession`, armazena o token internamente e consulta atualizacoes com `GetLatestConfiguration`.

## IAM da Task Role ou role da aplicacao

Permissao minima:

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

## Exemplo de configuracao hospedada no AppConfig

A biblioteca suporta:

1. Documento simples com `features`.
2. Hosted Feature Flags do AWS AppConfig lendo `values.<flag>.enabled`.

Exemplo:

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

## Endpoints das APIs de exemplo

As duas APIs expõem os mesmos endpoints:

- `GET /appconfig/raw`
- `GET /appconfig/features`
- `GET /appconfig/settings`
- `GET /simulation/home`
- `GET /simulation/checkout`
- `GET /simulation/reports`

`AwsAppConfig.AgentApi` responde no modo `Agent`.

`AwsAppConfig.StandardApi` responde no modo `Standard`.

## Executar localmente

Build da solucao:

```powershell
dotnet build AwsAppConfig.Poc.sln -c Release
```

Executar API com agent:

```powershell
dotnet run --project .\AwsAppConfig.AgentApi\AwsAppConfig.AgentApi.csproj
```

Executar API com acesso direto:

```powershell
dotnet run --project .\AwsAppConfig.StandardApi\AwsAppConfig.StandardApi.csproj
```

## Limites da PoC em relacao a DDD e arquitetura limpa

A solucao esta mais limpa e flexivel, mas continua sendo uma PoC de infraestrutura. Ela nao tem profundidade suficiente para justificar um modelo DDD completo com agregados, value objects, dominio rico e camadas separadas de aplicacao/dominio/infrastrutura. O que faz sentido aqui e:

- manter a biblioteca isolada como infraestrutura de configuracao;
- manter as APIs de exemplo finas;
- deixar regras de negocio reais em servicos de dominio nas aplicacoes consumidoras.

## Referencias oficiais AWS

- AWS AppConfig Agent para containers: https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-containers-agent.html
- Variaveis do agent (`POLL_INTERVAL`, `PREFETCH_LIST`, `SERVICE_REGION`): https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-integration-containers-agent-configuring.html
- Uso local do agent em `localhost:2772`: https://docs.aws.amazon.com/appconfig/latest/userguide/appconfig-agent-how-to-use-local-development-samples.html
- AppConfig Data API (`StartConfigurationSession`, `GetLatestConfiguration`): https://docs.aws.amazon.com/appconfig/2019-10-09/APIReference/Welcome.html
