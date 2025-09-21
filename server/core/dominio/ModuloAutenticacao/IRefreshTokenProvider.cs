using FluentResults;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface IRefreshTokenProvider
{
    public Task<Result<string>> GerarRefreshTokenAsync(Guid usuarioAutenticadoId, Guid tenantId, CancellationToken ct);
    public Task<Result<string>> GerarRefreshTokenAsync(Guid usuarioAutenticadoId, Guid tenantId, string? ip, string? userAgent, CancellationToken ct);
    public Task<Result<(Usuario Usuario, Guid TenantId, string NovoRefreshToken)>> RotacionarRefreshTokenAsync(string refreshTokenString, CancellationToken ct);
    public Task<Result> RevogarTokensUsuarioAsync(Guid usuarioAutenticadoId, CancellationToken ct);
    public string Hash(string valorEmTextoPlano);
    public string GerarTokenOpaco(int tamanhoEmBytes = 32);
}
