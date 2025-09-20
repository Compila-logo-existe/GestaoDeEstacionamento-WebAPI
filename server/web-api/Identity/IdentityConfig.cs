using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GestaoDeEstacionamento.WebAPI.Identity;

public static class IdentityConfig
{
    public static void AddIdentityProviders(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ITenantProvider, IdentityTenantProvider>();
        services.AddScoped<ITokenProvider, JwtProvider>();

        services.AddIdentity<Usuario, Cargo>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        string? chaveAssinaturaJwt = config["JWT_GENERATION_KEY"]
            ?? throw new ArgumentException("JWT_GENERATION_KEY não configurada.");

        byte[] chaveEmBytes = Encoding.UTF8.GetBytes(chaveAssinaturaJwt);

        string? audienciaValida = config["JWT_AUDIENCE_DOMAIN"]
            ?? throw new ArgumentException("JWT_AUDIENCE_DOMAIN não configurada.");

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.MapInboundClaims = false;
            x.RequireHttpsMetadata = true;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "GestaoEstacionamento",
                ValidateAudience = true,
                ValidAudience = audienciaValida,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(chaveEmBytes),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                RoleClaimType = "roles",
                NameClaimType = "unique_name"
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("PlatformAdminPolicy", p => p.RequireRole("PlataformaAdmin"))
            .AddPolicy("AdminPolicy", p => p.RequireRole("Admin"))
            .AddPolicy("UserPolicy", p => p.RequireRole("User"))
            .AddPolicy("AdminOrPlatformAdminPolicy", p => p.RequireRole("Admin", "PlataformaAdmin"));
    }
}
