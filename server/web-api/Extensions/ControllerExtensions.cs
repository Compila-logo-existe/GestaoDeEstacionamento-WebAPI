using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace GestaoDeEstacionamento.WebAPI.Helpers;

public static class ControllerExtensions
{
    public static ActionResult MapearFalha(this ControllerBase controller, Result result)
    {
        string[] mensagens = result.Errors
            .SelectMany(e => e.Reasons.OfType<IError>())
            .Select(e => e.Message)
            .ToArray();

        bool possuiTipo = result.Errors.Any(e => e.HasMetadataKey("TipoErro"));
        if (!possuiTipo)
            return controller.StatusCode(StatusCodes.Status500InternalServerError);

        string? tipo = result.Errors
            .SelectMany(e => e.Metadata)
            .Where(kvp => kvp.Key == "TipoErro")
            .Select(kvp => kvp.Value?.ToString())
            .FirstOrDefault();

        return tipo switch
        {
            "Conflito" => controller.Conflict(mensagens),   // 409
            "RegistroDuplicado" => controller.Conflict(mensagens),  // 409
            "ExclusaoBloqueada" => controller.Conflict(mensagens),  // 409
            "NaoEncontrado" => controller.NotFound(mensagens),  // 404
            "RegistroNaoEncontrado" => controller.NotFound(mensagens),  // 404
            "RequisicaoInvalida" => controller.BadRequest(mensagens),   // 400
            "NaoAutorizado" => controller.Unauthorized(),   // 401
            "Proibido" => controller.Forbid(),  // 403
            "ExcecaoInterna" => controller.StatusCode(500, mensagens),  // 500
            _ => controller.BadRequest(mensagens)   // fallback safe
        };
    }
}
