using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GestaoDeEstacionamento.WebAPI.Configuration;

public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation op, OperationFilterContext ctx)
    {
        string route = ctx.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
        string normalized = route.TrimStart('/');

        bool exigeHeader =
            normalized.StartsWith("auth/autenticar") ||
            normalized.StartsWith("auth/registrar");

        if (!exigeHeader)
            return;

        op.Parameters ??= new List<OpenApiParameter>();

        op.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Id",
            In = ParameterLocation.Header,
            Required = false,
            Description = "GUID do tenant (alternativa ao slug; só no login/registro).",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
            Example = new OpenApiString("11111111-1111-1111-1111-111111111111")
        });

        op.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Slug",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Slug do tenant (ex.: acme). Use GUID OU slug.",
            Schema = new OpenApiSchema { Type = "string" },
            Example = new OpenApiString("acme")
        });
    }
}
