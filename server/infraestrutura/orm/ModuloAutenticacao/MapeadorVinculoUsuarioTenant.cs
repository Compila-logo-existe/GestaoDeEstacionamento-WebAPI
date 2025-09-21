using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class MapeadorVinculoUsuarioTenant : IEntityTypeConfiguration<VinculoUsuarioTenant>
{
    public void Configure(EntityTypeBuilder<VinculoUsuarioTenant> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(v => v.TenantId)
            .IsRequired();

        builder.Property(v => v.UsuarioVinculadoId)
            .IsRequired();

        builder.Property(v => v.Slug)
            .IsRequired();

        builder.Property(v => v.NomeCargo)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(v => new { v.UsuarioVinculadoId, v.TenantId })
            .IsUnique();
    }
}
