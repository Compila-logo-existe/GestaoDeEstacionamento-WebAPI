using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class MapeadorConviteRegistro : IEntityTypeConfiguration<ConviteRegistro>
{
    public void Configure(EntityTypeBuilder<ConviteRegistro> b)
    {
        b.HasKey(c => c.Id);

        b.Property(c => c.Id)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(x => x.UsuarioEmissorId)
            .IsRequired();

        b.Property(c => c.TenantId)
            .IsRequired();

        b.Property(c => c.EmailConvidado)
            .HasMaxLength(256)
            .IsRequired();

        b.Property(c => c.NomeCargo)
            .HasMaxLength(64)
            .IsRequired();

        b.Property(c => c.TokenConvite)
            .HasMaxLength(128)
            .IsRequired();

        b.Property(c => c.DataExpiracaoUtc)
            .IsRequired();

        b.HasIndex(c => c.TokenConvite)
            .IsUnique();

        b.HasIndex(c => new { c.EmailConvidado, c.TenantId });
    }
}
