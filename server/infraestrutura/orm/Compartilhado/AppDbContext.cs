using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;

public class AppDbContext(DbContextOptions options, ITenantProvider? tenantProvider = null) : IdentityDbContext<Usuario, Cargo, Guid>(options), IUnitOfWork
{
    // Area para os DbSets, exemplo: public DbSet<Exemplo> Exemplos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (tenantProvider is not null)
        {
            /*
            Area para definir a seleção das queries via UsuarioId, exemplo: 

            modelBuilder.Entity<Contato>()
                .HasQueryFilter(x => x.UsuarioId.Equals(tenantProvider.UsuarioId));
            */
        }

        Assembly assembly = typeof(AppDbContext).Assembly;

        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        base.OnModelCreating(modelBuilder);
    }
    public async Task CommitAsync()
    {
        await SaveChangesAsync();
    }

    public async Task RollbackAsync()
    {
        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Unchanged;
                    break;

                case EntityState.Modified:
                    entry.State = EntityState.Unchanged;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }

        await Task.CompletedTask;
    }
}
