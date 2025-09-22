namespace GestaoDeEstacionamento.WebAPI.Models.ModuloAutenticacao;

public record CriarTenantRequest(
    string Nome,
    string? CNPJ,
    string SlugSubdominio,
    string? DominioPersonalizado
);
