using FluentResults;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao;

public class RefreshTokenProvider : IRefreshTokenProvider
{
    private readonly IRepositorioRefreshToken repositorio;
    private readonly IUnitOfWork unitOfWork;
    private readonly UserManager<Usuario> userManager;
    private readonly OpcoesRefreshToken opcoes;

    public RefreshTokenProvider(
        IRepositorioRefreshToken repositorio,
        IUnitOfWork unitOfWork,
        UserManager<Usuario> userManager,
        IOptions<OpcoesRefreshToken> opcoesAccessor)
    {
        this.repositorio = repositorio;
        this.unitOfWork = unitOfWork;
        this.userManager = userManager;
        opcoes = opcoesAccessor.Value;
    }

    public Task<Result<string>> GerarRefreshTokenAsync(Guid usuarioAutenticadoId, Guid tenantId, CancellationToken ct)
    {
        return GerarRefreshTokenAsync(usuarioAutenticadoId, tenantId, null, null, ct);
    }

    public async Task<Result<string>> GerarRefreshTokenAsync(
    Guid usuarioAutenticadoId, Guid tenantId, string? ip, string? userAgent, CancellationToken ct)
    {
        if (usuarioAutenticadoId == Guid.Empty || tenantId == Guid.Empty)
            return Result.Fail("Parâmetros inválidos.");

        string plain = GerarTokenOpaco();
        string hash = Hash(plain);

        RefreshToken entidade = new()
        {
            UsuarioAutenticadoId = usuarioAutenticadoId,
            HashDoToken = hash,
            CriadoEmUtc = DateTime.UtcNow,
            ExpiraEmUtc = DateTime.UtcNow.AddDays(opcoes.QuantidadeDiasDeValidade),
            EnderecoIpDeCriacao = ip,
            UserAgentDeCriacao = userAgent
        };
        entidade.VincularTenant(tenantId);

        await repositorio.CadastrarRegistroAsync(entidade);
        await unitOfWork.CommitAsync();

        return Result.Ok(plain);
    }

    public async Task<Result<(Usuario Usuario, Guid TenantId, string NovoRefreshToken)>> RotacionarRefreshTokenAsync(string refreshTokenString, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenString))
            return Result.Fail("Refresh token ausente.");

        string hash = Hash(refreshTokenString);
        RefreshToken? atual = await repositorio.SelecionarPorHashDoTokenAsync(hash, ct);
        if (atual is null) return Result.Fail("Refresh token inválido.");
        if (!atual.EstaAtivo) return Result.Fail("Refresh token expirado ou revogado.");

        string novoPlain = GerarTokenOpaco();
        string novoHash = Hash(novoPlain);

        atual.RevogadoEmUtc = DateTime.UtcNow;
        atual.SubstituidoPorHashDoToken = novoHash;
        await repositorio.EditarRegistroAsync(atual.Id, atual);

        RefreshToken novo = new()
        {
            UsuarioAutenticadoId = atual.UsuarioAutenticadoId,
            HashDoToken = novoHash,
            CriadoEmUtc = DateTime.UtcNow,
            ExpiraEmUtc = DateTime.UtcNow.AddDays(opcoes.QuantidadeDiasDeValidade)
        };
        novo.VincularTenant(atual.TenantId);

        await repositorio.CadastrarRegistroAsync(novo);
        await unitOfWork.CommitAsync();

        Usuario? usuario = await userManager.FindByIdAsync(atual.UsuarioAutenticadoId.ToString());
        if (usuario is null) return Result.Fail("Usuário do token não encontrado.");

        return Result.Ok((usuario, atual.TenantId, novoPlain));
    }

    public async Task<Result> RevogarTokensUsuarioAsync(Guid usuarioAutenticadoId, CancellationToken ct)
    {
        List<RefreshToken> tokens = await repositorio.SelecionarAtivosDoUsuarioAsync(usuarioAutenticadoId, ct);
        if (tokens.Count == 0) return Result.Ok();

        DateTime agora = DateTime.UtcNow;
        foreach (RefreshToken t in tokens)
        {
            t.RevogadoEmUtc = agora;
            await repositorio.EditarRegistroAsync(t.Id, t);
        }

        await unitOfWork.CommitAsync();
        return Result.Ok();
    }

    public string Hash(string valorEmTextoPlano)
    {
        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(opcoes.SegredoPepper));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(valorEmTextoPlano)));
    }

    public string GerarTokenOpaco(int tamanhoEmBytes = 32)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(tamanhoEmBytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}