using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao;

public class JwtProvider : ITokenProvider
{
    private readonly string audienciaValida;
    private readonly string chaveAssinaturaJwt;
    private readonly DateTime expiracaoJwt;

    public JwtProvider(IConfiguration config)
    {
        if (string.IsNullOrEmpty(config["JWT_GENERATION_KEY"]))
            throw new ArgumentException("Cifra de geração de tokens não configurada");

        chaveAssinaturaJwt = config["JWT_GENERATION_KEY"]!;

        if (string.IsNullOrEmpty(config["JWT_AUDIENCE_DOMAIN"]))
            throw new ArgumentException("Audiência válida para transmissão de tokens não configurada");

        audienciaValida = config["JWT_AUDIENCE_DOMAIN"]!;

        expiracaoJwt = DateTime.UtcNow.AddMinutes(15);
    }

    public AccessToken GerarAccessToken(Usuario usuario)
    {
        JwtSecurityTokenHandler tokenHandler = new();

        byte[] chaveEmBytes = Encoding.ASCII.GetBytes(chaveAssinaturaJwt!);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Issuer = "GestaoEstacionamento",
            Audience = audienciaValida,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email!)
            ]),
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
