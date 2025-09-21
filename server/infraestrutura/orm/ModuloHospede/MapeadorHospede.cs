using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloHospede;

public class MapeadorHospede : IEntityTypeConfiguration<Hospede>
{
    public void Configure(EntityTypeBuilder<Hospede> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(h => h.TenantId)
            .IsRequired();

        builder.Property(h => h.NomeCompleto)
            .IsRequired();

        builder.Property(h => h.CPF)
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(h => h.Telefone)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasMany(h => h.Veiculos)
               .WithOne(v => v.Hospede)
               .HasForeignKey(v => v.HospedeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(h => h.RegistrosEntrada)
               .WithOne(r => r.Hospede)
               .HasForeignKey(r => r.HospedeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(h => h.RegistrosSaida)
               .WithOne(r => r.Hospede)
               .HasForeignKey(r => r.HospedeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => new { h.TenantId, h.CPF })
            .IsUnique();
    }
}
