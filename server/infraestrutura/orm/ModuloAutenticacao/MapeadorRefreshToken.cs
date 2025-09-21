using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public sealed class MapeadorRefreshToken : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.UsuarioAutenticadoId)
            .IsRequired();

        builder.Property(x => x.HashDoToken)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.HashDoToken)
            .IsUnique();

        builder.Property(x => x.CriadoEmUtc)
            .IsRequired();

        builder.Property(x => x.ExpiraEmUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.UsuarioAutenticadoId, x.UsuarioId });
    }
}
