using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class MapeadorTicket : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(t => t.TenantId)
            .IsRequired();

        builder.Property(t => t.EmissaoEmUtc)
            .IsRequired();

        builder.Property(t => t.NumeroSequencial)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(1)
            .IsRequired();

        builder.HasIndex(t => t.NumeroSequencial)
            .IsUnique();
    }
}
