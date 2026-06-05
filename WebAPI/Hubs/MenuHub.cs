using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs
{
    public class MenuHub : Hub
    {
        private const string TenantQueryKey = "tenant";

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var tenantSlug = httpContext?.Request.Query[TenantQueryKey].ToString();

            if (!string.IsNullOrWhiteSpace(tenantSlug))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(tenantSlug));
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var tenantSlug = httpContext?.Request.Query[TenantQueryKey].ToString();

            if (!string.IsNullOrWhiteSpace(tenantSlug))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(tenantSlug));
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static string GetGroupName(string tenantSlug) => $"tenant:{tenantSlug}";

        public Task JoinTenantGroup(string tenantSlug)
        {
            if (string.IsNullOrWhiteSpace(tenantSlug))
            {
                throw new HubException("Tenant slug is required to join a group.");
            }

            return Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(tenantSlug));
        }
    }
}
