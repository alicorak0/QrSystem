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
using WebAPI.Middlewares;

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

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    string ResolveClientKey(HttpContext context)
    {
        var tenantSlug = context.Request.RouteValues["tenant"]?.ToString() ?? "global";
        var userName = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name
            : context.Request.Cookies["VisitorId"] ?? "anon";

        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = "anon";
        }
        Console.WriteLine(userName);

        return $"{tenantSlug}:{userName}";
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
            context.Response.StatusCode = 401;

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    Message = Messages.AuthenticationError,
                    StatusCode = 401
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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = 1
});

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
app.UseMiddleware<ExceptionMiddleware>();

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