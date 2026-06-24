using Core.CrossCuttingConcern.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace WebAPI.Middlewares
{
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
                    AuthenticationException => StatusCodes.Status401Unauthorized,
                    AuthorizationDeniedException => StatusCodes.Status403Forbidden,
                    ValidationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.StatusCode = statusCode;

                string message = exception.Message;

                if (exception is ValidationException validationException)
                {
                    message = validationException.Errors.FirstOrDefault()?.ErrorMessage ?? message;
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "Beklenmeyen bir hata olustu.";
                }

                var result = JsonSerializer.Serialize(new
                {
                    success = false,
                    message
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}