using System;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TenantMatchAttribute : AuthorizeAttribute
    {
        public TenantMatchAttribute()
        {
            Policy = "TenantMatch";
        }
    }
}
