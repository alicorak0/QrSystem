using DataAccess.Concrete.EntityFramework;
using System.Xml.Linq;

namespace WebAPI.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task Invoke(HttpContext context, MasterDbContext masterDb)
        {
            var path = context.Request.Path.Value?.ToLower();

            var isAuthRequest = path != null && path.Contains("/auth/login");

            var slug = context.Request.RouteValues["tenant"]?.ToString();

            if (!string.IsNullOrEmpty(slug))
            {
                var tenant = masterDb.Tenants
                    .FirstOrDefault(x => x.Slug == slug);

                if (tenant == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Tenant bulunamadı");
                    return;
                }

                // 🔥 DB SET
                context.Items["DatabaseName"] = tenant.DatabaseName;
                context.Items["TenantSlug"] = tenant.Slug;
                context.Items["TenantId"] = tenant.Id;

                // 🔐 USER TENANT CHECK (ASIL KRİTİK KISIM)
                var userTenantId = context.User?.FindFirst("tenant_id")?.Value;

                if (!isAuthRequest && !string.IsNullOrEmpty(userTenantId))
                {
                    if (userTenantId != tenant.Id.ToString())
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Tenant mismatch");
                        return;
                    }
                }

                Console.WriteLine($"Tenant: {tenant.Slug} | DB: {tenant.DatabaseName}");
            }

            await _next(context);
        }
    }
}
