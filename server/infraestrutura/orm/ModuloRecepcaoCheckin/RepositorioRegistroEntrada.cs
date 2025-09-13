using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class RepositorioRegistroEntrada(AppDbContext contexto)
    : RepositorioBaseORM<RegistroEntrada>(contexto), IRepositorioRegistroEntrada;
