using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using System.Security.Claims;

namespace GestaoDeEstacionamento.WebAPI.Identity;

public class IdentityTenantProvider(IHttpContextAccessor contextAccessor) : ITenantProvider
{
    public Guid? UsuarioId
    {
        get
        {
            Claim? claimId = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

            if (claimId is null)
                return null;

            return Guid.Parse(claimId.Value);
        }
    }

    public bool IsInRole(string role)
    {
        return contextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}
