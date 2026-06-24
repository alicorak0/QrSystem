using Business.Abstract;
using Business.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Business.Constants.DependencyResolvers.Autofac;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Core.Utilities.Security.JWT;
using Core.Utilities.Security.Encryption;
using Core.Extensions;
using Core.DependencyResolvers;
using System.Text.Json;
using Business.Constants;
using WebAPI.Hubs;
using Microsoft.EntityFrameworkCore;
using WebAPI.Middlewares;

// 🔥 SECURITY
using WebAPI.Security;
using Core.Utilities.IoC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var allowedCorsOrigins = new[]
{
    "http://localhost:4200",
    "https://localhost:4200",
    "https://nufusistatistikleri.online",
    "https://www.nufusistatistikleri.online",
     "https://efemkasapizgara.com",
                "http://efemkasapizgara.com",
                "https://www.efemkasapizgara.com"
};

// ---------------- SERVICES ----------------

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddResponseCaching();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// HttpContext
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 2;

    var knownProxies = builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>();
    if (knownProxies is not null)
    {
        foreach (var proxy in knownProxies)
        {
            if (IPAddress.TryParse(proxy, out var proxyIp))
            {
                options.KnownProxies.Add(proxyIp);
            }
        }
    }

    var knownNetworks = builder.Configuration.GetSection("ReverseProxy:KnownNetworks").Get<string[]>();
    if (knownNetworks is not null)
    {
        foreach (var network in knownNetworks)
        {
            var parts = network.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2
                && IPAddress.TryParse(parts[0], out var prefix)
                && int.TryParse(parts[1], out var prefixLength))
            {
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength));
            }
        }
    }
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            success = false,
            message = "Cok fazla istek gonderdiniz. Lutfen daha sonra tekrar deneyin."
        });

        await context.HttpContext.Response.WriteAsync(payload, token);
    };

    static string NormalizeIp(IPAddress ipAddress)
    {
        return ipAddress.IsIPv4MappedToIPv6
            ? ipAddress.MapToIPv4().ToString()
            : ipAddress.ToString();
    }

    static string ResolveClientIp(HttpContext context)
    {
        if (context.Connection.RemoteIpAddress is not null)
        {
            return NormalizeIp(context.Connection.RemoteIpAddress);
        }

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor)
            && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstIp = forwardedFor.ToString().Split(',')[0].Trim();
            if (IPAddress.TryParse(firstIp, out var parsedIp))
            {
                return NormalizeIp(parsedIp);
            }
        }

        return "unknown-ip";
    }

    string ResolveClientKey(HttpContext context)
    {
        const string rateLimiterLogOnceKey = "__RateLimiterKeyLogged";
        var tenantSlug = context.Request.RouteValues["tenant"]?.ToString() ?? "global";
        var visitorId = context.Request.Cookies["VisitorId"];
        var trimmedVisitorId = visitorId?.Trim();
        var usedVisitorId = !string.IsNullOrWhiteSpace(trimmedVisitorId);
        var clientIdentity = usedVisitorId
            ? $"visitor:{trimmedVisitorId}"
            : $"ip:{ResolveClientIp(context)}";
        var partitionKey = $"{tenantSlug}:{clientIdentity}";

        if (!context.Items.ContainsKey(rateLimiterLogOnceKey))
        {
            Console.WriteLine(
                $"[RateLimiter] {context.Request.Method} {context.Request.Path} Trace={context.TraceIdentifier} source={(usedVisitorId ? "VisitorId" : "IP")} key={partitionKey}");
            context.Items[rateLimiterLogOnceKey] = true;
        }

        return partitionKey;
    }

    var defaultLimiterOptions = new SlidingWindowRateLimiterOptions
    {
        PermitLimit = 200,
        Window = TimeSpan.FromMinutes(1),
        SegmentsPerWindow = 6,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 0,
        AutoReplenishment = true
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var partitionKey = ResolveClientKey(httpContext);
        return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => defaultLimiterOptions);
    });

    options.AddPolicy("high", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(ResolveClientKey(httpContext), _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 40,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        })
    );

    options.AddPolicy("medium", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(ResolveClientKey(httpContext), _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10,
            AutoReplenishment = true
        })
    );

    options.AddPolicy("low", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(ResolveClientKey(httpContext), _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        })
    );
});

// DbContext
builder.Services.AddDbContextFactory<QrMenuContext>();

builder.Services.AddDbContextFactory<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Base"))
);

// Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterModule(new AutofacBusinessModule());
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedCorsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---------------- JWT ----------------

var tokenOptions = builder.Configuration.GetSection("TokenOptions")
    .Get<TokenOptions>()
    ?? throw new InvalidOperationException("TokenOptions missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("access_token"))
            {
                context.Token = context.Request.Cookies["access_token"];
            }
            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    success = false,
                    message = Messages.AuthenticationError
                })
            );
        },

        OnForbidden = context =>
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    success = false,
                    message = Messages.AuthorizationDenied
                })
            );
        }
    };
});

// ---------------- AUTHORIZATION ----------------

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantMatch", policy =>
    {
        policy.Requirements.Add(new TenantMatchRequirement());
    });
});

// Handler DI
builder.Services.AddSingleton<IAuthorizationHandler, TenantMatchHandler>();

// Core DI
builder.Services.AddDependencyResolvers(new ICoreModule[]
{
    new CoreModule()
});

// ---------------- APP BUILD ----------------

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseForwardedHeaders();

app.UseRouting();

// CORS
app.UseCors("FrontendCorsPolicy");

// 🔥 AUTH
app.UseAuthentication();

// 🔥 Her anonim ziyaretçi için tekil kimlik oluşturuyoruz
app.UseMiddleware<VisitorIdMiddleware>();

// static files
app.UseStaticFiles();

app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"), app =>
{
    app.UseRateLimiter();
});

// 🔥 TENANT MIDDLEWARE (DB SELECT)
app.UseMiddleware<TenantMiddleware>();

// 🔥 AUTHORIZATION (POLICIES RUN HERE)
app.UseAuthorization();

// Exception handling
app.UseMiddleware<WebAPI.Middlewares.ExceptionMiddleware>();

// caching
app.UseResponseCaching();

// SignalR
app.MapHub<MenuHub>("/menuhub")
    .DisableRateLimiting();

// controllers
app.MapControllers();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();