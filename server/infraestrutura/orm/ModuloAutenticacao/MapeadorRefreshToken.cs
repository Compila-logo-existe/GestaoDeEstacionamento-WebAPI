using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public sealed class MapeadorRefreshToken : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.Property(r => r.UsuarioAutenticadoId)
            .IsRequired();

        builder.Property(r => r.HashDoToken)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(r => r.HashDoToken)
            .IsUnique();

        builder.Property(r => r.CriadoEmUtc)
            .IsRequired();

        builder.Property(r => r.ExpiraEmUtc)
            .IsRequired();

        builder.HasIndex(r => new { r.UsuarioAutenticadoId, r.TenantId });
    }
}
