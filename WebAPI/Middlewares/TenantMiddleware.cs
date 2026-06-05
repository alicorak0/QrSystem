using DataAccess.Concrete.EntityFramework;

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

                // 🔥 SADECE CONTEXT SET
                context.Items["DatabaseName"] = tenant.DatabaseName;
                context.Items["TenantSlug"] = tenant.Slug;
                context.Items["TenantId"] = tenant.Id;
            }
            else
            {
                // 🔥 MASTER DB CASE
                context.Items["DatabaseName"] = "QrMenuMaster";
                context.Items["TenantSlug"] = null;
                context.Items["TenantId"] = null;
            }

            await _next(context);
        }
    }
}