using Backend.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected IActionResult ApiOk<T>(T data) => Ok(ApiResponse<T>.Ok(data));

    protected IActionResult ApiOk<T>(T data, Pagination pagination) =>
        Ok(ApiResponse<T>.Ok(data, pagination));

    protected IActionResult ApiFail<T>(string message, string code, object? details = null) =>
        BadRequest(ApiResponse<T>.Fail(message, code, details));
}
