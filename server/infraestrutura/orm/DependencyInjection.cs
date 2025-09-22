using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloEstacionamento;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloFaturamento;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloHospede;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoDeEstacionamento.Infraestrutura.ORM;

public static class DependencyInjection
{
    public static IServiceCollection AddCamadaInfraestruturaOrm(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRepositorioHospede, RepositorioHospede>();
        services.AddScoped<IRepositorioVeiculo, RepositorioVeiculo>();
        services.AddScoped<IRepositorioEstacionamento, RepositorioEstacionamento>();
        services.AddScoped<IRepositorioVaga, RepositorioVaga>();
        services.AddScoped<IRepositorioRegistroEntrada, RepositorioRegistroEntrada>();
        services.AddScoped<IRepositorioTicket, RepositorioTicket>();
        services.AddScoped<IRepositorioRegistroSaida, RepositorioRegistroSaida>();
        services.AddScoped<IRepositorioFaturamento, RepositorioFaturamento>();
        services.AddScoped<IRepositorioTenant, RepositorioTenant>();
        services.AddScoped<IRepositorioUsuarioTenant, RepositorioUsuarioTenant>();
        services.AddScoped<IRepositorioConvite, RepositorioConviteRegistro>();
        services.AddScoped<IRepositorioRefreshToken, RepositorioRefreshToken>();

        services.AddEntityFrameworkConfig(configuration);

        return services;
    }

    private static void AddEntityFrameworkConfig(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string? connectionString = configuration["SQL_CONNECTION_STRING"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new Exception("A variável SQL_CONNECTION_STRING não foi fornecida.");

        services.AddDbContext<IUnitOfWork, AppDbContext>(options =>
            options.UseNpgsql(connectionString, (opt) => opt.EnableRetryOnFailure(3)));
    }
}
