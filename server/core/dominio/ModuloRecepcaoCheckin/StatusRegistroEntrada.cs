using System.ComponentModel.DataAnnotations;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public enum StatusRegistroEntrada
{
    [Display(Name = "Livre")]
    Livre,
    [Display(Name = "Ocupada")]
    Ocupada
}