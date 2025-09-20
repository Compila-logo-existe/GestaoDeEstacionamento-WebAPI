using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace GestaoDeEstacionamento.WebAPI.Identity;

public sealed class IdentityTenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    private const string ClaimTypeSubject = "sub";
    private const string ClaimTypeNameId = "nameid";
    private const string ClaimTypeNameIdentifier = ClaimTypes.NameIdentifier;

    private const string ClaimTypeTenantId = "tenant_id";
    private const string ClaimTypeTenantSlug = "tenant_slug";

    private const string HeaderTenantId = "X-Tenant-Id";
    private const string HeaderTenantSlug = "X-Tenant-Slug";

    private const string ItemsResolvedTenantIdKey = "ResolvedTenantId";

    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

    public Guid? UsuarioId
    {
        get
        {
            ClaimsPrincipal? claimsPrincipal = httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal?.Identity?.IsAuthenticated != true)
                return null;

            string? rawIdentifier =
                claimsPrincipal.FindFirst(ClaimTypeSubject)?.Value ??
                claimsPrincipal.FindFirst(ClaimTypeNameId)?.Value ??
                claimsPrincipal.FindFirst(ClaimTypeNameIdentifier)?.Value;

            return TryParseGuid(rawIdentifier);
        }
    }

    public Guid? TenantId
    {
        get
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            ClaimsPrincipal? claimsPrincipal = httpContext?.User;

            // 1) Autenticado: prioriza o claim "tenant_id"
            string? tenantIdFromClaim = claimsPrincipal?.FindFirst(ClaimTypeTenantId)?.Value;
            Guid? tenantIdParsedFromClaim = TryParseGuid(tenantIdFromClaim);
            if (tenantIdParsedFromClaim.HasValue)
                return tenantIdParsedFromClaim.Value;

            // 2) Resolvido por middleware (host -> tenant)
            if (httpContext != null &&
                httpContext.Items.TryGetValue(ItemsResolvedTenantIdKey, out object? resolved) &&
                resolved is Guid resolvedGuid &&
                resolvedGuid != Guid.Empty)
            {
                return resolvedGuid;
            }

            // 3) Header X-Tenant-Id (aceito apenas para rotas anônimas de login/registro)
            bool isAuthenticated = claimsPrincipal?.Identity?.IsAuthenticated ?? false;
            if (httpContext != null &&
                !isAuthenticated)
            {
                string? tenantIdFromHeader = GetFirstHeaderValueOrNull(httpContext.Request.Headers, HeaderTenantId);
                Guid? tenantIdParsedFromHeader = TryParseGuid(tenantIdFromHeader);
                if (tenantIdParsedFromHeader.HasValue)
                    return tenantIdParsedFromHeader.Value;
            }

            return null;
        }
    }

    public string? Slug
    {
        get
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
                return null;

            ClaimsPrincipal? claimsPrincipal = httpContext.User;

            // 1) tenta pelo claim
            string? normalizedClaimSlug = NormalizeSlug(
                claimsPrincipal?.FindFirst(ClaimTypeTenantSlug)?.Value
            );

            // 2) tenta pelo header (mantendo sua ordem: claim primeiro, header depois)
            string? headerSlug = GetFirstHeaderValueOrNull(httpContext.Request.Headers, HeaderTenantSlug);
            string? normalizedHeaderSlug = NormalizeSlug(headerSlug);

            // se houver claim usa ele; senão, usa o header
            return normalizedClaimSlug ?? normalizedHeaderSlug;
        }
    }

    public bool IsInRole(string roleName)
    {
        return httpContextAccessor.HttpContext?.User?.IsInRole(roleName) ?? false;
    }

    private static Guid? TryParseGuid(string? value)
    {
        if (Guid.TryParse(value, out Guid guid))
            return guid;
        return null;
    }

    private static string? NormalizeSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToLowerInvariant();
    }

    private static string? GetFirstHeaderValueOrNull(IHeaderDictionary headers, string headerName)
    {
        if (!headers.TryGetValue(headerName, out StringValues values))
            return null;

        if (values.Count > 0)
            return values[0];

        return null;
    }
}
