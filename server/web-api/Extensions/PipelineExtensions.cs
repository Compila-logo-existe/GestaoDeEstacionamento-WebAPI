namespace GestaoDeEstacionamento.WebAPI.Extensions;

public static class PipelineExtensions
{
    /// <summary>
    /// Mantém a ordem de pipeline que você adotou: ExceptionHandler → Https → HostTenantResolution → CORS → Auth → Authorization → Controllers (+/health)
    /// </summary>
    public static WebApplication UseWebApiPipelineDefaults(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseMiddleware<HostTenantResolutionMiddleware>();
        app.UseCors("spa");
        app.UseAuthentication();
        app.UseAuthorization();

        // Endpoints
        app.MapControllers();
        app.MapHealthChecks("/health/live");
        app.MapHealthChecks("/health/ready");

        return app;
    }
}
