using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace GestaoDeEstacionamento.Testes.API;

public class TestWebAPIFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        SQLitePCL.Batteries.Init();

        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<AppDbContext>)) ?? throw new Exception("DbContextOptions<AppDbContext> not found");

            services.Remove(descriptor);

            services.AddSingleton<DbConnection>(_ =>
            {
                SqliteConnection connection = new("DataSource=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<AppDbContext>((container, options) =>
            {
                DbConnection? connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });
    }
}
