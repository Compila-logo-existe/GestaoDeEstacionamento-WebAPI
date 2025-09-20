using GestaoDeEstacionamento.Core.Aplicacao;
using GestaoDeEstacionamento.Infraestrutura.ORM;
using GestaoDeEstacionamento.WebAPI.AutoMapper;
using GestaoDeEstacionamento.WebAPI.Configuration;
using GestaoDeEstacionamento.WebAPI.Extensions;
using GestaoDeEstacionamento.WebAPI.Identity;
using System.Text.Json.Serialization;

namespace GestaoDeEstacionamento.WebAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Camadas
        builder.Services
            .AddCamadaAplicacao(builder.Logging, builder.Configuration)
            .AddCamadaInfraestruturaOrm(builder.Configuration);

        // Infra de WebAPI (mapeamento, identidade, swagger, etc.)
        builder.Services.AddAutoMapperProfiles(builder.Configuration);
        builder.Services.AddIdentityProviders();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddSwaggerConfig();

        // Controllers + enums como string
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Fundações da Web API (ProblemDetails, CORS, Versionamento, HealthChecks)
        builder.Services.AddWebApiBasics(builder.Configuration);

        WebApplication app = builder.Build();

        // One-off tasks: executa e encerra
        if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
        {
            app.ApplyMigrations();
            return;
        }

        if (args.Contains("--seed-platform", StringComparer.OrdinalIgnoreCase))
        {
            app.ApplyMigrations();
            await app.SeedPlataformaAsync();
            return;
        }

        // Desenvolvimento: migra/seed + Swagger na raiz
        if (app.Environment.IsDevelopment())
        {
            app.ApplyMigrations();
            await app.SeedPlataformaAsync();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty; // Swagger na raiz
            });
        }

        // Pipeline padrão da API 
        app.UseWebApiPipelineDefaults();

        await app.RunAsync();
    }
}
