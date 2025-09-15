using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloEstacionamento;

public class RepositorioEstacionamento(AppDbContext contexto)
    : RepositorioBaseORM<Estacionamento>(contexto), IRepositorioEstacionamento
{
}
