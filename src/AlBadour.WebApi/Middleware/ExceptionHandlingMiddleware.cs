using System.Net;
using System.Text.Json;
using AlBadour.Domain.Exceptions;
using FluentValidation;

namespace AlBadour.WebApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("VALIDATION_ERROR",
                    string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
                    validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToArray())
            ),
            UnauthorizedActionException unauthorizedEx => (
                HttpStatusCode.Forbidden,
                new ErrorResponse("FORBIDDEN", unauthorizedEx.Message, null)
            ),
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(domainEx.Code, domainEx.Message, null)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("INTERNAL_ERROR", "An unexpected error occurred.", null)
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception: {Type} - {Message}", exception.GetType().Name, exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}

public record ErrorResponse(string Code, string Message, object? Errors);
