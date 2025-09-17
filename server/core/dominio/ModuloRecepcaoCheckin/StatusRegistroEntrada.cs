using System.ComponentModel.DataAnnotations;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public enum StatusRegistroEntrada
{
    [Display(Name = "Válida")]
    Valida,
    [Display(Name = "Expirada")]
    Expirada
}