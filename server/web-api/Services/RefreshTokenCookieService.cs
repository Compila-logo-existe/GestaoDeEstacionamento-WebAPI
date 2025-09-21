using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.Extensions.Options;

namespace GestaoDeEstacionamento.WebAPI.Services;

public sealed class RefreshTokenCookieService : IRefreshTokenCookieService
{
    private readonly IHttpContextAccessor http;
    private readonly OpcoesRefreshToken opcoes;

    public RefreshTokenCookieService(IHttpContextAccessor httpContextAccessor, IOptions<OpcoesRefreshToken> opcoesAccessor)
    {
        http = httpContextAccessor;
        opcoes = opcoesAccessor.Value;
    }

    public void Gravar(string valor, int dias, bool crossSite)
    {
        HttpContext? ctx = http.HttpContext;
        if (ctx is null || string.IsNullOrWhiteSpace(opcoes.NomeDoCookie) || string.IsNullOrWhiteSpace(valor)) return;

        CookieOptions opt = new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/auth/refresh",
            Expires = DateTimeOffset.UtcNow.AddDays(dias),
            IsEssential = true
        };
        ctx.Response.Cookies.Append(opcoes.NomeDoCookie, valor, opt);
    }

    public string? Ler()
    {
        HttpContext? ctx = http.HttpContext;
        if (ctx is null || string.IsNullOrWhiteSpace(opcoes.NomeDoCookie)) return null;
        return ctx.Request.Cookies.TryGetValue(opcoes.NomeDoCookie, out string? v) ? v : null;
    }

    public void Remover()
    {
        HttpContext? ctx = http.HttpContext;
        if (ctx is null || string.IsNullOrWhiteSpace(opcoes.NomeDoCookie)) return;

        ctx.Response.Cookies.Delete(opcoes.NomeDoCookie);

        void Expirar(string path, SameSiteMode sameSite, bool secure)
        {
            CookieOptions opt = new()
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = sameSite,
                Path = path,
                Expires = DateTimeOffset.UnixEpoch,
                IsEssential = true
            };

            ctx.Response.Cookies.Append(opcoes.NomeDoCookie, string.Empty, opt);
        }

        string[] paths = ["/auth/refresh", "/auth", "/"];
        SameSiteMode[] sites = [SameSiteMode.None, SameSiteMode.Lax, SameSiteMode.Strict];

        foreach (string p in paths)
            foreach (SameSiteMode s in sites)
            {
                Expirar(p, s, secure: true);
                Expirar(p, s, secure: false);
            }
    }
}
