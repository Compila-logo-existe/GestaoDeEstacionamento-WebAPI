using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;

public class AppDbContext(DbContextOptions options, ITenantProvider? tenantProvider = null) : IdentityDbContext<Usuario, Cargo, Guid>(options), IUnitOfWork
{
    public DbSet<Hospede> Hospedes { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }
    public DbSet<RegistroEntrada> RegistrosEntrada { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Vaga> Vagas { get; set; }
    public DbSet<Estacionamento> Estacionamentos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (tenantProvider is not null)
        {
            modelBuilder.Entity<Hospede>()
                .HasQueryFilter(x => x.UsuarioId.Equals(tenantProvider.UsuarioId));

            modelBuilder.Entity<Veiculo>()
                .HasQueryFilter(x => x.UsuarioId.Equals(tenantProvider.UsuarioId));

            modelBuilder.Entity<RegistroEntrada>()
                .HasQueryFilter(x => x.UsuarioId.Equals(tenantProvider.UsuarioId));

            modelBuilder.Entity<Ticket>()
                .HasQueryFilter(x => x.UsuarioId.Equals(tenantProvider.UsuarioId));

            modelBuilder.Entity<Vaga>()
                .HasQueryFilter(x => x.UsuarioId.Equals(tenantProvider.UsuarioId));

            modelBuilder.Entity<Estacionamento>()
                .HasQueryFilter(x => x.UsuarioId.Equals(tenantProvider.UsuarioId));
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
