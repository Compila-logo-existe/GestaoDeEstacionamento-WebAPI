using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoDeEstacionamento.Infraestrutura.ORM;

public static class DependencyInjection
{
    public static IServiceCollection AddCamadaInfraestruturaOrm(this IServiceCollection services, IConfiguration configuration)
    {
        // Area para adicionar os Scopeds, exemplo: services.AddScoped<IRepositorioExemplo, RepositorioExemploORM>();

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
