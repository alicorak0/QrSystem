using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult FromResult(Core.Utilities.Results.IResult result, int errorStatusCode = StatusCodes.Status400BadRequest)
        {
            if (result.Success)
            {
                return Success(data: null, message: result.Message);
            }

            return Error(result.Message, errorStatusCode);
        }

        protected IActionResult FromDataResult<T>(Core.Utilities.Results.IDataResult<T> result, int errorStatusCode = StatusCodes.Status400BadRequest)
        {
            if (result.Success)
            {
                return Success(result.Data, result.Message);
            }

            return Error(result.Message, errorStatusCode);
        }

        protected IActionResult Success(object? data, string? message)
        {
            return Ok(new
            {
                success = true,
                data,
                message = NormalizeMessage(message, "Islem basarili.")
            });
        }

        protected IActionResult Error(string? message, int statusCode = StatusCodes.Status400BadRequest)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message = NormalizeMessage(message, "Bir hata olustu.")
            });
        }

        private static string NormalizeMessage(string? message, string fallback)
        {
            return string.IsNullOrWhiteSpace(message) ? fallback : message;
        }
    }
}
