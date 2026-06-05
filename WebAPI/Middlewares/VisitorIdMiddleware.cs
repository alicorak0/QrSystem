using Microsoft.AspNetCore.Http;

namespace WebAPI.Middlewares
{
    public class VisitorIdMiddleware
    {
        private const string VisitorIdCookieName = "VisitorId";
        private readonly RequestDelegate _next;

        public VisitorIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Cookies.ContainsKey(VisitorIdCookieName))
            {
                var visitorId = Guid.NewGuid().ToString();
                context.Response.Cookies.Append(VisitorIdCookieName, visitorId, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = context.Request.IsHttps,
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                });
            }

            await _next(context);
        }
    }
}
