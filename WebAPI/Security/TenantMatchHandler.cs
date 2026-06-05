using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebAPI.Security;

public class TenantMatchHandler : AuthorizationHandler<TenantMatchRequirement>
{
    private readonly IHttpContextAccessor _http;

    public TenantMatchHandler(IHttpContextAccessor http)
    {
        _http = http;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantMatchRequirement requirement)
    {
        Console.WriteLine("TENANT HANDLER WORKING");

        var http = _http.HttpContext;

        if (http == null)
            return Task.CompletedTask;
        //        var routeTenantId = http.Request.RouteValues["tenant"]?.ToString();

        var routeTenantId = http.Items["TenantId"]?.ToString();

        if (string.IsNullOrEmpty(routeTenantId))
            return Task.CompletedTask;

        var userTenantId = context.User.FindFirst("tenant_id")?.Value;

        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var isSuperAdmin = string.Equals(role, "superadmin", StringComparison.OrdinalIgnoreCase);

        // 🔥 SUPERADMIN RULE
        if (isSuperAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 🔥 NORMAL USER RULE
        if (!string.IsNullOrEmpty(userTenantId) &&
            userTenantId == routeTenantId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}