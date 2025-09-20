// Middlewares/HostTenantResolutionMiddleware.cs
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public sealed class HostTenantResolutionMiddleware
{
    private readonly RequestDelegate next;

    public HostTenantResolutionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    // ITenantRepositorio e IConfiguration são resolvidos POR REQUEST (scoped/any)
    public async Task Invoke(HttpContext ctx, ITenantRepositorio repo, IConfiguration cfg)
    {
        // Se já resolvido (ou autenticado com claim), segue
        if (ctx.Items.ContainsKey("ResolvedTenantId") ||
            (ctx.User?.Identity?.IsAuthenticated ?? false))
        {
            await next(ctx);
            return;
        }

        // 1) Header X-Tenant-Id (GUID)
        if (ctx.Request.Headers.TryGetValue("X-Tenant-Id", out Microsoft.Extensions.Primitives.StringValues idHeader) &&
            Guid.TryParse(idHeader.ToString(), out Guid parsed))
        {
            ctx.Items["ResolvedTenantId"] = parsed;
            await next(ctx);
            return;
        }

        // 2) Header X-Tenant-Slug (string legível)
        if (ctx.Request.Headers.TryGetValue("X-Tenant-Slug", out Microsoft.Extensions.Primitives.StringValues slugHeader))
        {
            string slug = slugHeader.ToString().Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(slug))
            {
                // ⚠️ Usa repositório no Invoke (nada no ctor)
                Guid? tenantId = await repo.ObterTenantIdPorSubdominioAsync(slug, ctx.RequestAborted);
                if (tenantId != null)
                {
                    ctx.Items["ResolvedTenantId"] = tenantId.Value;
                    await next(ctx);
                    return;
                }
            }
        }

        // 3) (já existia) resolução por host/subdomínio
        Guid? resolvedFromHost = await repo.ObterTenantIdPorDominioAsync(ctx.Request.Host.Host, ctx.RequestAborted);
        if (resolvedFromHost.HasValue)
            ctx.Items["ResolvedTenantId"] = resolvedFromHost.Value;

        await next(ctx);
    }
}
