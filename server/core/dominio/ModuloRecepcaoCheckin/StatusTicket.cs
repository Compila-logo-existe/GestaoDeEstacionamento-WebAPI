using System.ComponentModel.DataAnnotations;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public enum StatusTicket
{
    [Display(Name = "Válido")]
    Valido,
    [Display(Name = "Expirado")]
    Expirado
}