using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloEstacionamento;

public class MapeadorEstacionamento : IEntityTypeConfiguration<Estacionamento>
{
    public void Configure(EntityTypeBuilder<Estacionamento> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.Nome)
            .IsRequired();

        builder.HasMany(e => e.Vagas)
            .WithOne(v => v.Estacionamento)
            .HasForeignKey(v => v.EstacionamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UsuarioId, e.Nome })
            .IsUnique();
    }
}
