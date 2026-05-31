using Core.CrossCuttingConcern.Exceptions;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";

            int statusCode = exception switch
            {
                AuthenticationException => 401,
                AuthorizationDeniedException => 403,
                _ => 500
            };

            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(new
            {
                Message = exception.Message,
                StatusCode = statusCode
            });

            await context.Response.WriteAsync(result);
        }
    }
}