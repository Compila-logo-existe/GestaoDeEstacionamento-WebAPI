using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class MapeadorRegistroEntrada : IEntityTypeConfiguration<RegistroEntrada>
{
    public void Configure(EntityTypeBuilder<RegistroEntrada> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.DataEntradaEmUtc)
            .IsRequired();

        builder.Property(r => r.DataSaidaEmUtc)
            .IsRequired(false);

        builder.HasOne(r => r.Hospede)
            .WithMany(h => h.RegistrosEntrada)
            .HasForeignKey(r => r.HospedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Ticket)
            .WithOne(t => t.RegistroEntrada)
            .HasForeignKey<RegistroEntrada>(t => t.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Veiculo)
            .WithMany(h => h.RegistrosEntrada)
            .HasForeignKey(r => r.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.Observacoes)
            .HasMaxLength(1000);

        builder.HasIndex(r => r.TicketId)
            .IsUnique();

        builder.HasIndex(r => r.DataEntradaEmUtc);

        builder.HasIndex(r => r.DataSaidaEmUtc);
    }
}
