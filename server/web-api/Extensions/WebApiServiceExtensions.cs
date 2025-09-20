namespace GestaoDeEstacionamento.WebAPI.Extensions;

public static class WebApiServiceExtensions
{
    public static IServiceCollection AddWebApiBasics(this IServiceCollection services, IConfiguration configuration)
    {
        // ProblemDetails com enriquecimento de tenant e traceId
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                HttpContext http = ctx.HttpContext;
                string? tenantId = http.User?.FindFirst("tenant_id")?.Value;

                if (string.IsNullOrWhiteSpace(tenantId) &&
                    http.Items.TryGetValue("ResolvedTenantId", out object? val))
                {
                    tenantId = val?.ToString();
                }

                ctx.ProblemDetails.Extensions["traceId"] = http.TraceIdentifier;
                if (!string.IsNullOrWhiteSpace(tenantId))
                    ctx.ProblemDetails.Extensions["tenantId"] = tenantId;
            };
        });

        // CORS (SPA)
        string[] allowedOrigins = (configuration["CORS_ALLOWED_ORIGINS"] ?? "")
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(opt =>
        {
            opt.AddPolicy("spa", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration["SQL_CONNECTION_STRING"] ?? "");

        return services;
    }
}
