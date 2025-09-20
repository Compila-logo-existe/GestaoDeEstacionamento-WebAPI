using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao;

public class JwtProvider : ITokenProvider
{
    private readonly UserManager<Usuario> userManager;
    private readonly string audienciaValida;
    private readonly string chaveAssinaturaJwt;

    public JwtProvider(IConfiguration config, UserManager<Usuario> userManager)
    {
        if (string.IsNullOrEmpty(config["JWT_GENERATION_KEY"]))
            throw new ArgumentException("JWT_GENERATION_KEY não configurada");

        chaveAssinaturaJwt = config["JWT_GENERATION_KEY"]!;

        if (string.IsNullOrEmpty(config["JWT_AUDIENCE_DOMAIN"]))
            throw new ArgumentException("JWT_AUDIENCE_DOMAIN não configurada");

        audienciaValida = config["JWT_AUDIENCE_DOMAIN"]!;

        this.userManager = userManager;
    }

    public async Task<AccessToken> GerarAccessToken(
        Usuario usuario,
        Guid tenantId
    )
    {
        DateTime expiracaoJwt = DateTime.UtcNow.AddMinutes(15);

        JwtSecurityTokenHandler tokenHandler = new();

        byte[] chaveEmBytes = Encoding.UTF8.GetBytes(chaveAssinaturaJwt!);

        IList<string> userRoles = await userManager.GetRolesAsync(usuario);

        List<Claim> claims = new()
        {
            new("sub", usuario.Id.ToString()),
            new("unique_name", usuario.UserName ?? usuario.Email ?? usuario.Id.ToString()),
            new("email", usuario.Email ?? string.Empty),
            new("tenant_id", tenantId.ToString() ?? string.Empty)
        };

        foreach (string role in userRoles)
            claims.Add(new Claim("roles", role));

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Issuer = "GestaoEstacionamento",
            Audience = audienciaValida,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(chaveEmBytes),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Expires = expiracaoJwt
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        string tokenString = tokenHandler.WriteToken(token);

        return new AccessToken(
            tokenString,
            expiracaoJwt,
            new UsuarioAutenticado(
                usuario.Id,
                usuario.FullName ?? string.Empty,
                usuario.Email ?? string.Empty
            )
        );
    }
}
