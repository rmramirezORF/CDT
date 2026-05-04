using Backend.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Api.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        context.Result = new BadRequestObjectResult(
            ApiResponse<object>.Fail("Datos de entrada inválidos", "VALIDATION_ERROR", errors));
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    /// <summary>
    /// Misma forma de respuesta que la del filtro,
    /// pero usable como InvalidModelStateResponseFactory.
    /// </summary>
    public static IActionResult BuildResponse(ActionContext context)
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(
            ApiResponse<object>.Fail("Datos de entrada inválidos", "VALIDATION_ERROR", errors));
    }
}
