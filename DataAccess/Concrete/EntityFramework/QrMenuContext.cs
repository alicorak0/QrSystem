using Core.Entities.Concrete;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace DataAccess.Concrete.EntityFramework
{
    public class QrMenuContext:DbContext
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public QrMenuContext(
        DbContextOptions<QrMenuContext> options,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
        : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            var dbName = _httpContextAccessor.HttpContext?.Items["DatabaseName"]?.ToString();
            var baseConn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? _configuration.GetConnectionString("Base");

            if (!string.IsNullOrWhiteSpace(baseConn))
            {
                var builder = new SqlConnectionStringBuilder(baseConn);

                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    builder.InitialCatalog = dbName;
                }

                optionsBuilder.UseSqlServer(builder.ConnectionString);
            }
            else
            {
                throw new InvalidOperationException("Base database connection string is missing. Set CONNECTION_STRING env var or ConnectionStrings:Base.");
            }

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories{ get; set; }

        public DbSet<User> Users{ get; set; }   

        public DbSet<OperationClaim> OperationClaims{ get; set; }   

        public DbSet<UserOperationClaims> UserOperationClaims{ get; set; }

        public DbSet<FeaturedProducts> FeaturedProducts { get; set; }
    }
}
