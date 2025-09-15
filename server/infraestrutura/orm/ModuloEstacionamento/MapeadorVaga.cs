using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloEstacionamento;

public class MapeadorVaga : IEntityTypeConfiguration<Vaga>
{
    public void Configure(EntityTypeBuilder<Vaga> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.Numero)
            .IsRequired();

        builder.Property(v => v.Zona)
            .IsRequired();

        builder.HasOne(v => v.VeiculoEstacionado)
            .WithOne(v => v.Vaga)
            .HasForeignKey<Vaga>(v => v.VeiculoId);

        builder.Ignore(v => v.Status);

        builder.HasOne(v => v.Estacionamento)
            .WithMany(e => e.Vagas)
            .HasForeignKey(v => v.EstacionamentoId)
            .IsRequired();

        builder.HasIndex(v => new { v.EstacionamentoId, v.Zona, v.Numero })
            .IsUnique();
    }
}
