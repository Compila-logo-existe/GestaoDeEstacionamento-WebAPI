namespace GestaoDeEstacionamento.WebAPI.Extensions;

public static class PipelineExtensions
{
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
