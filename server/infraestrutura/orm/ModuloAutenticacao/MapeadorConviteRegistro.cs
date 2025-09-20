using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class MapeadorConviteRegistro : IEntityTypeConfiguration<ConviteRegistro>
{
    public void Configure(EntityTypeBuilder<ConviteRegistro> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.UsuarioId)
            .IsRequired();

        b.Property(x => x.TenantId)
            .IsRequired();

        b.Property(x => x.EmailConvidado)
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.NomeCargo)
            .HasMaxLength(64)
            .IsRequired();

        b.Property(x => x.TokenConvite)
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.DataExpiracaoUtc)
            .IsRequired();

        b.HasIndex(x => x.TokenConvite)
            .IsUnique();

        b.HasIndex(x => new { x.EmailConvidado, x.TenantId });
    }
}
