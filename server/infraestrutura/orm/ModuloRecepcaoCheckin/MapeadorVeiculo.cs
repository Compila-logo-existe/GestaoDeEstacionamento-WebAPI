using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class MapeadorVeiculo : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(v => v.Placa)
            .IsRequired();

        builder.Property(v => v.Modelo)
            .IsRequired();

        builder.Property(v => v.Cor)
            .IsRequired();

        builder.HasOne(v => v.Hospede)
            .WithMany(h => h.Veiculos)
            .HasForeignKey(v => v.HospedeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.Placa)
            .IsUnique();
    }
}