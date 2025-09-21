using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloFaturamento;

public class MapeadorFaturamento : IEntityTypeConfiguration<Faturamento>
{
    public void Configure(EntityTypeBuilder<Faturamento> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(f => f.TenantId)
            .IsRequired();

        builder.Property(f => f.TenantId)
            .IsRequired();

        builder.Property(f => f.ValorDaDiaria)
            .HasPrecision(14, 2)
            .IsRequired();

        builder.Property(f => f.NumeroDeDiarias)
            .IsRequired();

        builder.Property(f => f.ValorTotal)
            .HasPrecision(14, 2)
            .IsRequired();

        builder.Property(f => f.DataEntradaEmUtc)
            .IsRequired();

        builder.HasOne(f => f.RegistroSaida)
            .WithOne(r => r.Faturamento)
            .HasForeignKey<Faturamento>(f => f.RegistroSaidaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.RegistroEntrada)
            .WithOne(r => r.Faturamento)
            .HasForeignKey<Faturamento>(f => f.RegistroEntradaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.DataEntradaEmUtc);

        builder.HasIndex(f => new { f.TenantId, f.RegistroSaidaId }).IsUnique();
    }
}
