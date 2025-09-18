namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

public static class MensagensErroAutenticacao
{
    public const string UsuarioJaExiste = "Já existe um usuário com esse nome.";
    public const string EmailJaExiste = "Já existe um usuário com esse e-mail.";
    public const string SenhaMuitoCurta = "A senha é muito curta.";
    public const string SenhaRequerCaracterEspecial = "A senha deve conter pelo menos um caractere especial.";
    public const string SenhaRequerNumero = "A senha deve conter pelo menos um número.";
    public const string SenhaRequerMaiuscula = "A senha deve conter pelo menos uma letra maiúscula.";
    public const string SenhaRequerMinuscula = "A senha deve conter pelo menos uma letra minúscula.";
    public const string ContaBloqueada = "Sua conta foi bloqueada temporariamente devido a muitas tentativas inválidas.";
    public const string LoginNaoPermitido = "Não é permitido efetuar login. Verifique se sua conta está confirmada.";
    public const string RequerAutenticacaoDoisFatores = "É necessário confirmar o login com autenticação de dois fatores.";
    public const string DadosInvalidos = "Login ou senha incorretos.";
}
