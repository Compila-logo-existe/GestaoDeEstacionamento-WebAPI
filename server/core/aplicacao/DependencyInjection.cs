using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Reflection;

namespace GestaoDeEstacionamento.Core.Aplicacao;

public static class DependencyInjection
{
    public static IServiceCollection AddCamadaAplicacao(
        this IServiceCollection services,
        ILoggingBuilder logging,
        IConfiguration configuration
    )
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        string? luckyPennySoftwareLicenseKey = configuration["LUCKYPENNYSOFTWARE_LICENSE_KEY"];

        if (string.IsNullOrWhiteSpace(luckyPennySoftwareLicenseKey))
            throw new Exception("A variável LUCKYPENNYSOFTWARE_LICENSE_KEY não foi fornecida.");

        services.AddSerilogConfig(logging, configuration);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            config.LicenseKey = luckyPennySoftwareLicenseKey;
        });

        services.AddAutoMapper(config => config.LicenseKey = luckyPennySoftwareLicenseKey, assembly);

        services.AddValidatorsFromAssembly(assembly);

        string? redisConnectionString = configuration["REDIS_CONNECTION_STRING"];

        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = redisConnectionString;
            option.InstanceName = "eagenda";
        });

        return services;
    }

    private static void AddSerilogConfig(this IServiceCollection services, ILoggingBuilder logging, IConfiguration configuration)
    {
        string? licenseKey = configuration["NEWRELIC_LICENSE_KEY"];

        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new Exception("A variável NEWRELIC_LICENSE_KEY não foi fornecida.");

        string caminhoAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string caminhoArquivoLogs = Path.Combine(caminhoAppData, "eAgenda", "erro.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(caminhoArquivoLogs, LogEventLevel.Error)
            .WriteTo.NewRelicLogs(
                endpointUrl: "https://log-api.newrelic.com/log/v1",
                applicationName: "e-agenda-app",
                licenseKey: licenseKey
            )
            .CreateLogger();

        logging.ClearProviders();

        services.AddSerilog();
    }
}
