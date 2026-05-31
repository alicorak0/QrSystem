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
    }
}
