using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Identity;

public static class SeedExtensions
{
    public static async Task SeedPlataformaAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SeedPlataforma");

        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        string? platformEmail = configuration["PLATFORM_ADMIN_EMAIL"];
        string? platformPassword = configuration["PLATFORM_ADMIN_PASSWORD"];
        string? platformFullName = configuration["PLATFORM_ADMIN_FULLNAME"];

        if (string.IsNullOrWhiteSpace(platformEmail) ||
            string.IsNullOrWhiteSpace(platformPassword) ||
            string.IsNullOrWhiteSpace(platformFullName))
        {
            throw new InvalidOperationException(
                "SeedPlataforma: faltam variáveis obrigatórias. Defina PLATFORM_ADMIN_EMAIL, PLATFORM_ADMIN_PASSWORD e PLATFORM_ADMIN_FULLNAME.");
        }

        RoleManager<Cargo> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Cargo>>();
        UserManager<Usuario> userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        string[] roles = { "PlataformaAdmin", "Admin", "User" };
        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                IdentityResult createRole = await roleManager.CreateAsync(new Cargo { Name = role });
                if (!createRole.Succeeded)
                    throw new InvalidOperationException(
                        $"Falha ao criar role '{role}': {string.Join("; ", createRole.Errors.Select(e => e.Description))}");
            }
        }

        Usuario? usuario = await userManager.FindByEmailAsync(platformEmail);
        if (usuario is null)
        {
            usuario = new Usuario
            {
                UserName = platformEmail,
                Email = platformEmail,
                EmailConfirmed = true,
                FullName = platformFullName
            };

            IdentityResult createUser = await userManager.CreateAsync(usuario, platformPassword);
            if (!createUser.Succeeded)
                throw new InvalidOperationException(
                    $"Falha ao criar usuário PlataformaAdmin: {string.Join("; ", createUser.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(usuario, "PlataformaAdmin");
            logger.LogInformation("SeedPlataforma: usuário {Email} criado com role PlataformaAdmin.", platformEmail);
        }
        else
        {
            if (!await userManager.IsInRoleAsync(usuario, "PlataformaAdmin"))
                await userManager.AddToRoleAsync(usuario, "PlataformaAdmin");

            logger.LogInformation("SeedPlataforma: usuário {Email} já existe (senha não alterada).", platformEmail);
        }
    }
}
