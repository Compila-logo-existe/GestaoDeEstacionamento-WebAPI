using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class MapeadorTenant : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(t => t.UsuarioCriadorId)
            .IsRequired();

        builder.Ignore(x => x.TenantId);

        builder.Property(t => t.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.CNPJ)
            .HasMaxLength(32);

        builder.Property(t => t.CriadoEmUtc)
            .IsRequired();

        builder.Property(t => t.Ativo)
            .IsRequired();

        builder.Property(t => t.SlugSubdominio).IsRequired().HasMaxLength(63);
        builder.Property(t => t.DominioPersonalizado).HasMaxLength(253);

        builder.HasIndex(t => t.SlugSubdominio)
            .IsUnique();

        builder.HasIndex(t => t.DominioPersonalizado)
            .IsUnique();

        builder.HasIndex(t => t.Nome);
    }
}
