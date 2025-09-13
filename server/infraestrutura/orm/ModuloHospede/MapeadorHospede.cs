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
            .ValueGeneratedNever();

        builder.Property(h => h.NomeCompleto)
            .IsRequired();

        builder.Property(h => h.CPF)
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(h => h.Telefone)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(h => h.CPF)
            .IsUnique();
    }
}
