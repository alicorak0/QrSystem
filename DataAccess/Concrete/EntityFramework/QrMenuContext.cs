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
                return;

            var dbName = _httpContextAccessor.HttpContext?
                .Items["DatabaseName"]?.ToString();

            var baseConn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? _configuration.GetConnectionString("Base");

            var builder = new SqlConnectionStringBuilder(baseConn);

            // 🔥 SAFE FALLBACK
            builder.InitialCatalog = string.IsNullOrEmpty(dbName)
                ? "QrMenuMaster"
                : dbName;

            optionsBuilder.UseSqlServer(builder.ConnectionString);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductAllergen>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ProductAllergen>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ProductAllergen>()
                .HasIndex(x => new { x.ProductId, x.AllergenId })
                .IsUnique();
        }


        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories{ get; set; }

        public DbSet<User> Users{ get; set; }   

        public DbSet<OperationClaim> OperationClaims{ get; set; }   

        public DbSet<UserOperationClaims> UserOperationClaims{ get; set; }

        public DbSet<FeaturedProducts> FeaturedProducts { get; set; }

        //Alerjen ve içerik tabloları
        public DbSet<Allergen> Allergens { get; set; }

        public DbSet<ProductAllergen> ProductAllergens { get; set; }

        public DbSet<Ingredient> Ingredients { get; set; }

    }
}
