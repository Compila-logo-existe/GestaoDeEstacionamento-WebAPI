using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.WebAPI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GestaoDeEstacionamento.Testes.API.ModuloAutenticacao;

[TestClass]
public abstract class TestFixture
{
    public static HttpClient httpClient = null!;
    public static string tenantId = "11111111-1111-1111-1111-111111111111";
    public static string tenantSlug = string.Empty;
    public static string bearerToken = string.Empty;
    private static TestWebAPIFactory<Program> factory = null!;
    private static readonly IConfiguration config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
    private static readonly CredenciaisAutenticacao mainAccount = new()
    {
        Email = config["PLATFORM_ADMIN_EMAIL"]!,
        Senha = config["PLATFORM_ADMIN_PASSWORD"]!,
    };

    [AssemblyInitialize]
    public static async Task Setup(TestContext _)
    {
        factory = new TestWebAPIFactory<Program>();
        httpClient = factory.CreateClient();

        await AuthMain();
        await CadastrarTenant();
        ResetHeaders();
    }

    private static async Task CadastrarTenant()
    {
        CriarTenantCommand tenant = new(
            "Estacionamento Teste",
            "12.345.678/0001-90",
            "eteste",
            null
        );

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/platform/tenants", tenant);

        TenantResult? result = await response.Content.ReadFromJsonAsync<TenantResult>();
        tenantId = result?.TenantId.ToString() ?? string.Empty;
        tenantSlug = result?.TenantSlug ?? string.Empty;
    }

    private static async Task AuthMain()
    {
        AutenticarUsuarioCommand authCommand = new(
            mainAccount.Email,
            mainAccount.Senha,
            null,
            null
        );
        httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/auth/autenticar", authCommand);

        AccessToken? accessToken = await response.Content.ReadFromJsonAsync<AccessToken>();
        bearerToken = accessToken!.Chave;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    private static void ResetHeaders()
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        httpClient.DefaultRequestHeaders.Add("X-Tenant-Slug", tenantSlug);
        bearerToken = string.Empty;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }
}

public sealed class CredenciaisAutenticacao
{
    public required string Email { get; init; }
    public required string Senha { get; init; }
}
