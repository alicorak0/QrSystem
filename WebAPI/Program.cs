using Business.Abstract;
using Business.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Business.Constants.DependencyResolvers.Autofac;
using Autofac.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Core.Utilities.Security.JWT;
using Core.Utilities.Security.Encryption;
using Core.Extensions;
using Core.Utilities.IoC;
using Core.DependencyResolvers;
using System.Text.Json;
using Business.Constants;
using WebAPI.Hubs;
using Microsoft.EntityFrameworkCore;
using WebAPI.Middlewares;


var builder = WebApplication.CreateBuilder(args);
var allowedCorsOrigins = new[]
{
    "http://localhost:4200",
        "https://localhost:4200",

    "https://nufusistatistikleri.online",
    "https://www.nufusistatistikleri.online"
};

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

//browser caching
builder.Services.AddResponseCaching();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




//WebSocket
builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContextFactory<QrMenuContext>();

builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Base")
    ));







builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterModule(new AutofacBusinessModule());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
              "https://localhost:4200",
    "https://alicorak0.github.io", // EKLE BUNU
    "http://alicorak0.github.io", // EKLE BUNU

                "https://nufusistatistikleri.online",
                "https://www.nufusistatistikleri.online"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>()
    ?? throw new InvalidOperationException("TokenOptions configuration is missing.");


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


               // BURASI EKLEND�
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
                       context.HandleResponse(); // default challenge i�lemini engelle
                       context.Response.ContentType = "application/json";
                       context.Response.StatusCode = 401;
                       return context.Response.WriteAsync(
                           JsonSerializer.Serialize(new
                           {
                               Message = Messages.AuthenticationError, // kendi mesaj�n
                               StatusCode = 401
                           })
                       );
                   }
               };
               




           });

builder.Services.AddDependencyResolvers(new ICoreModule[]{
    new CoreModule()

    }); //��eriye eklenecek mod�lleri gireriz



var app = builder.Build();

app.MapHub<MenuHub>("/menuhub");
app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


//app.ConfigureCustomExceptionMiddleware();

app.UseRouting();


app.UseMiddleware<TenantMiddleware>(); // 🔥 BURASI

app.UseCors("FrontendCorsPolicy");
app.UseResponseCaching(); // 🔥 BURAYA

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();

app.UseAuthorization();


app.UseStaticFiles();   //   for image upload



app.MapControllers();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
