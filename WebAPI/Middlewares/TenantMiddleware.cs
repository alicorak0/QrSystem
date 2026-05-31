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
            var slug = context.Request.RouteValues["tenant"]?.ToString();

            if (!string.IsNullOrEmpty(slug))
            {
                var tenant = masterDb.Tenants
                    .FirstOrDefault(x => x.Slug == slug);

                if (tenant != null)
                {
                    // 🔥 DB için
                    context.Items["DatabaseName"] = tenant.DatabaseName;

                    // 🔥 Upload & URL için
                    context.Items["TenantSlug"] = tenant.Slug;

                    Console.WriteLine($"Tenant: {tenant.Slug} | DB: {tenant.DatabaseName}");
                }
                else
                {
                    Console.WriteLine("Tenant bulunamadı!");
                }
            }

            await _next(context);
        }
    }
}
