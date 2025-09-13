using System.Text.RegularExpressions;

namespace GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;

public static class Padronizador
{
    public static string PadronizarCPF(string? cPF)
    {
        return string.IsNullOrWhiteSpace(cPF)
            ? string.Empty : Regex.Replace(cPF, "[^0-9]", "");
    }
    public static string PadronizarPlaca(string? placa)
    {
        return string.IsNullOrWhiteSpace(placa)
            ? string.Empty : Regex.Replace(placa, "[^A-Za-z0-9]", "")
            .ToUpperInvariant();
    }
}
