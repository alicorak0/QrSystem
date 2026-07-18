using DataAccess.Abstract;
using Core.Entities;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core.DataAccess.EntityFramework;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using Core.Utilities.Results;
using System.Transactions;


namespace DataAccess.Concrete.EntityFramework
{
    //Product dal şablonu içerir EfEntityreposritorybase implemente eder diğer tüm enities erişibilir.
    //EfPRODUCT dal ilse kendi  entity türünü kullanır  Repoda ortaklar  tutulur
    public class EfProductDal : EfEntityRepositoryBase<Product, QrMenuContext>, IProductDal
    {
        private readonly IDbContextFactory<QrMenuContext> _contextFactory;

        public EfProductDal(IDbContextFactory<QrMenuContext> contextFactory)
            : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<Product> GetByCategoryName(string categoryName)
        {
            using var context = _contextFactory.CreateDbContext();
            var category = context.Categories
                .FirstOrDefault(c => c.CategoryName.ToLower() == categoryName.ToLower());

            if (category == null)
                return new List<Product>();

            return context.Products
                .Where(p => p.CategoryId == category.CategoryId)
                .ToList();
        }

        public List<ProductDto> GetAllProductDtos()
        {
            using var context = _contextFactory.CreateDbContext();
            return CreateProductDtoQuery(context).ToList();
        }

        public List<ProductDto> GetProductDtosByCategoryId(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return CreateProductDtoQuery(context)
                .Where(p => p.CategoryId == categoryId)
                .ToList();
        }

        public List<ProductDto> GetProductDtosByCategoryName(string categoryName)
        {
            using var context = _contextFactory.CreateDbContext();
            var normalized = categoryName.Trim().ToLower();

            var categoryId = context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryName.ToLower() == normalized)
                .Select(c => (int?)c.CategoryId)
                .FirstOrDefault();

            if (!categoryId.HasValue)
            {
                return new List<ProductDto>();
            }

            return CreateProductDtoQuery(context)
                .Where(p => p.CategoryId == categoryId.Value)
                .ToList();
        }

        public int AddWithRelations(ProductSaveDto productSaveDto)
        {
            using var context = _contextFactory.CreateDbContext();
            var hasAmbientTransaction = Transaction.Current != null;
            using var transaction = hasAmbientTransaction ? null : context.Database.BeginTransaction();

            var product = new Product
            {
                CategoryId = productSaveDto.CategoryId,
                ProductName = productSaveDto.ProductName,
                Description = productSaveDto.Description,
                Tooltip = productSaveDto.Tooltip,
                Price = productSaveDto.Price,
                Image = productSaveDto.Image,
                IsFeatured = productSaveDto.IsFeatured
            };

            context.Products.Add(product);
            context.SaveChanges();

            var ingredientNames = NormalizeIngredientNames(productSaveDto.IngredientNames);
            var allergenIds = NormalizeAllergenIds(productSaveDto.AllergenIds);

            if (ingredientNames.Count > 0)
            {
                var ingredients = ingredientNames
                    .Select(name => new Ingredient { ProductId = product.ProductId, Name = name })
                    .ToList();

                context.Ingredients.AddRange(ingredients);
            }

            if (allergenIds.Count > 0)
            {
                var productAllergens = allergenIds
                    .Select(allergenId => new ProductAllergen
                    {
                        ProductId = product.ProductId,
                        AllergenId = allergenId
                    })
                    .ToList();

                context.ProductAllergens.AddRange(productAllergens);
            }

            context.SaveChanges();
            transaction?.Commit();

            return product.ProductId;
        }

        public bool UpdateWithRelations(int productId, ProductSaveDto productSaveDto)
        {
            using var context = _contextFactory.CreateDbContext();
            var hasAmbientTransaction = Transaction.Current != null;
            using var transaction = hasAmbientTransaction ? null : context.Database.BeginTransaction();

            var product = context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return false;
            }

            product.CategoryId = productSaveDto.CategoryId;
            product.ProductName = productSaveDto.ProductName;
            product.Description = productSaveDto.Description;
            product.Tooltip = productSaveDto.Tooltip;
            product.Price = productSaveDto.Price;
            product.Image = productSaveDto.Image;
            product.IsFeatured = productSaveDto.IsFeatured;

            var oldIngredients = context.Ingredients.Where(i => i.ProductId == productId).ToList();
            if (oldIngredients.Count > 0)
            {
                context.Ingredients.RemoveRange(oldIngredients);
            }

            var oldProductAllergens = context.ProductAllergens.Where(pa => pa.ProductId == productId).ToList();
            if (oldProductAllergens.Count > 0)
            {
                context.ProductAllergens.RemoveRange(oldProductAllergens);
            }

            var ingredientNames = NormalizeIngredientNames(productSaveDto.IngredientNames);
            var allergenIds = NormalizeAllergenIds(productSaveDto.AllergenIds);

            if (ingredientNames.Count > 0)
            {
                var ingredients = ingredientNames
                    .Select(name => new Ingredient { ProductId = productId, Name = name })
                    .ToList();

                context.Ingredients.AddRange(ingredients);
            }

            if (allergenIds.Count > 0)
            {
                var productAllergens = allergenIds
                    .Select(allergenId => new ProductAllergen
                    {
                        ProductId = productId,
                        AllergenId = allergenId
                    })
                    .ToList();

                context.ProductAllergens.AddRange(productAllergens);
            }

            context.SaveChanges();
            transaction?.Commit();

            return true;
        }

        public bool DeleteWithRelations(int productId)
        {
            using var context = _contextFactory.CreateDbContext();
            var hasAmbientTransaction = Transaction.Current != null;
            using var transaction = hasAmbientTransaction ? null : context.Database.BeginTransaction();

            var product = context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return false;
            }

            var ingredients = context.Ingredients.Where(i => i.ProductId == productId).ToList();
            if (ingredients.Count > 0)
            {
                context.Ingredients.RemoveRange(ingredients);
            }

            var productAllergens = context.ProductAllergens.Where(pa => pa.ProductId == productId).ToList();
            if (productAllergens.Count > 0)
            {
                context.ProductAllergens.RemoveRange(productAllergens);
            }

            context.Products.Remove(product);
            context.SaveChanges();
            transaction?.Commit();

            return true;
        }

        public List<ProductDetailDto> GetProductDetails()
        {
            using var context = _contextFactory.CreateDbContext();
            var result = from p in context.Products
                         join c in context.Categories
                             on p.CategoryId equals c.CategoryId
                         select new ProductDetailDto
                         {
                             ProductId = p.ProductId,
                             ProductName = p.ProductName,
                             Price = p.Price,
                             CategoryName = c.CategoryName,
                             Tooltip = p.Tooltip,
                             Description = p.Description,
                             İmage = p.Image
                         };

            return result.ToList();
        }

        private static IQueryable<ProductDto> CreateProductDtoQuery(QrMenuContext context)
        {
            return context.Products
                .AsNoTracking()
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Tooltip = p.Tooltip,
                    Price = p.Price,
                    Image = p.Image,
                    IsFeatured = p.IsFeatured,
                    IngredientNames = p.Ingredients
                        .OrderBy(i => i.Id)
                        .Select(i => i.Name)
                        .ToList(),
                    Allergens = p.ProductAllergens
                        .OrderBy(pa => pa.Allergen.Name)
                        .Select(pa => new AllergenDto
                        {
                            AllergenId = pa.AllergenId,
                            Name = pa.Allergen.Name,
                            Icon = pa.Allergen.Icon
                        })
                        .ToList()
                });
        }

        private static List<string> NormalizeIngredientNames(List<string>? ingredientNames)
        {
            return (ingredientNames ?? new List<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<int> NormalizeAllergenIds(List<int>? allergenIds)
        {
            return (allergenIds ?? new List<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }
    }
}
