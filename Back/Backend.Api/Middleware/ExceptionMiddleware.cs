using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Application.Common.DTOs;
using Backend.Application.Common.Exceptions;

namespace Backend.Api.Middleware;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException bex)
        {
            await WriteResponse(context, HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(bex.Message, bex.Code, bex.Details));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await WriteResponse(context, HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("Error interno", "UNKNOWN_ERROR"));
        }
    }

    private static Task WriteResponse(HttpContext context, HttpStatusCode status, ApiResponse<object> body)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}
