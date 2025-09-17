using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class MapeadorRegistroSaida : IEntityTypeConfiguration<RegistroSaida>
{
    public void Configure(EntityTypeBuilder<RegistroSaida> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.DataSaidaEmUtc)
            .IsRequired(false);

        builder.HasOne(r => r.Hospede)
            .WithMany(h => h.RegistrosSaida)
            .HasForeignKey(r => r.HospedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Ticket)
            .WithOne(t => t.RegistroSaida)
            .HasForeignKey<RegistroSaida>(r => r.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Veiculo)
            .WithMany(v => v.RegistrosSaida)
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

        builder.HasIndex(r => r.DataSaidaEmUtc);

        builder.HasIndex(r => new { r.UsuarioId, r.TicketId }).IsUnique();
    }
}
