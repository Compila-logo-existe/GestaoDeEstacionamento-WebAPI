using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.WebAPI.ORM;

public static class DatabaseOperations
{
    public static void ApplyMigrations(this IHost app)
    {
        IServiceScope scope = app.Services.CreateScope();

        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Database.Migrate();
    }
}
