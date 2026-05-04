namespace Backend.Application.Common.DTOs;

/// <summary>
/// Envelope estandar de toda respuesta del API.
/// </summary>
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public Pagination? Pagination { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ApiError? Error { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Data = data, Success = true };

    public static ApiResponse<T> Ok(T data, Pagination pagination) =>
        new() { Data = data, Pagination = pagination, Success = true };

    public static ApiResponse<T> Fail(string message, string code, object? details = null) =>
        new() { Success = false, Message = message, Error = new ApiError(code, details) };
}

public class ApiError
{
    public string Code { get; set; }
    public object? Details { get; set; }

    public ApiError(string code, object? details = null)
    {
        Code = code;
        Details = details;
    }
}

public class Pagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalRecords { get; set; }
}
