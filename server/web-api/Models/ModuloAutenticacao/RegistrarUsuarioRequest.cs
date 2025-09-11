namespace GestaoDeEstacionamento.WebAPI.Models.ModuloAutenticacao;

public record RegistrarUsuarioRequest(
    string NomeCompleto,
    string Email,
    string Senha,
    string ConfirmarSenha
);
