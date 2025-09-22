using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class MapeadorRegistroEntrada : IEntityTypeConfiguration<RegistroEntrada>
{
    public void Configure(EntityTypeBuilder<RegistroEntrada> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.Property(r => r.DataEntradaEmUtc)
            .IsRequired();

        builder.HasOne(r => r.Hospede)
            .WithMany(h => h.RegistrosEntrada)
            .HasForeignKey(r => r.HospedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Ticket)
            .WithOne(t => t.RegistroEntrada)
            .HasForeignKey<RegistroEntrada>(t => t.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Veiculo)
            .WithMany(h => h.RegistrosEntrada)
            .HasForeignKey(r => r.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.Observacoes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrWhiteSpace(v)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("text")
            .HasDefaultValueSql("'[]'");

        builder.HasIndex(r => r.DataEntradaEmUtc);

        builder.HasIndex(r => new { r.TenantId, r.TicketId }).IsUnique();
    }
}
