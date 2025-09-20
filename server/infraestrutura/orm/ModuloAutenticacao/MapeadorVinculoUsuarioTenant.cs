using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class MapeadorVinculoUsuarioTenant : IEntityTypeConfiguration<VinculoUsuarioTenant>
{
    public void Configure(EntityTypeBuilder<VinculoUsuarioTenant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Slug)
            .IsRequired();

        builder.Property(x => x.NomeCargo)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => new { x.UsuarioId, x.TenantId })
            .IsUnique();
    }
}
