using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class MapeadorTenant : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CNPJ)
            .HasMaxLength(32);

        builder.Property(x => x.CriadoEmUtc)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.SlugSubdominio).IsRequired().HasMaxLength(63);
        builder.Property(x => x.DominioPersonalizado).HasMaxLength(253);

        builder.HasIndex(x => x.SlugSubdominio)
            .IsUnique();

        builder.HasIndex(x => x.DominioPersonalizado)
            .IsUnique();

        builder.HasIndex(x => x.Nome);
    }
}
