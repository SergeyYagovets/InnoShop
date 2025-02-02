using UserManagement.Domain.Exceptions;

namespace UserManagement.Presentation.Middleware;

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
            _logger.LogError(ex, "An unhandled exception occurred.");
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        switch (exception)
        {
            case ArgumentException argEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return context.Response.WriteAsJsonAsync(new
                {
                    context.Response.StatusCode,
                    Message = $"Bad Request: {argEx.Message}"
                });

            case UnauthorizedAccessException unauthorizedEx:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsJsonAsync(new
                {
                    context.Response.StatusCode,
                    Message = $"Unauthorized: {unauthorizedEx.Message}"
                });

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                return context.Response.WriteAsJsonAsync(new
                {
                    context.Response.StatusCode,
                    Message = $"Conflict: {invalidOpEx.Message}"
                });

            case NotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return context.Response.WriteAsJsonAsync(new
                {
                    context.Response.StatusCode,
                    Message = $"Not Found: {notFoundEx.Message}"
                });

            case TimeoutException timeoutEx:
                context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                return context.Response.WriteAsJsonAsync(new
                {
                    context.Response.StatusCode,
                    Message = $"Request Timeout: {timeoutEx.Message}"
                });

            case Exception ex:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return context.Response.WriteAsJsonAsync(new
                {
                    context.Response.StatusCode,
                    Message = $"An unexpected error occurred: {ex.Message}"
                });
        }
        return Task.CompletedTask;
    }
}
