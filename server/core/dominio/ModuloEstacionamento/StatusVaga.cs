using System.ComponentModel.DataAnnotations;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

public enum StatusVaga
{
    [Display(Name = "Livre")]
    Livre,
    [Display(Name = "Ocupada")]
    Ocupada
}