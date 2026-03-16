using AwsAppConfig.Ecs;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AWS AppConfig PoC API",
        Version = "v1",
        Description =
            "API de demonstracao para consumo de configuracoes e feature flags do AWS AppConfig. " +
            "A mesma API expõe rotas separadas para os cenarios Agent e Standard para facilitar o entendimento e o teste."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});
builder.Services.AddProblemDetails();
builder.Services.AddAwsAppConfigEcs(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "AWS AppConfig PoC API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AWS AppConfig PoC API v1");
    });
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();
app.Run();
