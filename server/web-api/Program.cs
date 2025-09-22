using GestaoDeEstacionamento.Core.Aplicacao;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;
using GestaoDeEstacionamento.WebAPI.AutoMapper;
using GestaoDeEstacionamento.WebAPI.Configuration;
using GestaoDeEstacionamento.WebAPI.Extensions;
using GestaoDeEstacionamento.WebAPI.Identity;
using GestaoDeEstacionamento.WebAPI.Services;
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

        builder.Services.Configure<OpcoesRefreshToken>(builder.Configuration.GetSection("OpcoesRefreshToken"));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IRefreshTokenCookieService, RefreshTokenCookieService>();
        builder.Services.AddScoped<IRepositorioRefreshToken, RepositorioRefreshToken>();
        builder.Services.AddScoped<IRefreshTokenProvider, RefreshTokenProvider>();

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
