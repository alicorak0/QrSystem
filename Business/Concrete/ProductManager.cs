using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IProductDal _productDal;
        private readonly ICategoryService _categoryService;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IR2StorageService _r2StorageService;

        public ProductManager(
            IProductDal productDal,
            ICategoryService categoryService,
            IMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor,
            IR2StorageService r2StorageService)
        {
            _productDal = productDal;
            _categoryService = categoryService;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _r2StorageService = r2StorageService;
        }

        [ValidationAspect(typeof(ProductSaveDtoValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public IResult Add(ProductSaveDto product)
        {
            product.IngredientNames = NormalizeIngredientNames(product.IngredientNames);
            product.AllergenIds = NormalizeAllergenIds(product.AllergenIds);
            product.Image = NormalizeImage(product.Image);

            var results = BusinessRules.Run(
                CheckIfProductNameExists(product.ProductName),
                CheckIfProductCountOfCategoryError(product.CategoryId)
            );

            if (results != null)
            {
                return results;
            }

            _productDal.AddWithRelations(product);
            return new SuccessResult(Messages.ProductAdded);
        }

        [CacheAspect]
        public IDataResult<List<ProductDto>> GetAll()
        {
            return new SuccessDataResult<List<ProductDto>>(_productDal.GetAllProductDtos(), Messages.ProductListed);
        }

        [CacheAspect]
        public IDataResult<List<ProductDto>> GetAllByCategory(int id)
        {
            return new SuccessDataResult<List<ProductDto>>(_productDal.GetProductDtosByCategoryId(id));
        }

        [CacheAspect]
        [PerformanceAspect(2)]
        public IDataResult<Product> GetById(int id)
        {
            return new SuccessDataResult<Product>(_productDal.Get(p => p.ProductId == id));
        }

        [CacheRemoveAspect("IProductService.Get")]
        public IResult Delete(int id)
        {
            var productToDelete = _productDal.Get(p => p.ProductId == id);
            if (productToDelete == null)
            {
                return new ErrorResult("Urun bulunamadi");
            }

            var tenantSlug = _httpContextAccessor.HttpContext?.Items["TenantSlug"]?.ToString();
            if (!string.IsNullOrWhiteSpace(tenantSlug)
                && !string.IsNullOrWhiteSpace(productToDelete.Image)
                && !IsCommonImage(productToDelete.Image))
            {
                _r2StorageService.DeleteProductImageAsync(tenantSlug, productToDelete.Image)
                    .GetAwaiter()
                    .GetResult();
            }

            var deleted = _productDal.DeleteWithRelations(id);
            if (!deleted)
            {
                return new ErrorResult("Urun bulunamadi");
            }

            return new SuccessResult("Urun silindi");
        }

        [CacheAspect]
        public IDataResult<List<ProductDetailDto>> GetProductDetails()
        {
            if (DateTime.Now.Hour == 0)
            {
                return new ErrorDataResult<List<ProductDetailDto>>(Messages.MaintanenceTime);
            }

            return new SuccessDataResult<List<ProductDetailDto>>(_productDal.GetProductDetails());
        }

        [ValidationAspect(typeof(ProductSaveDtoValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        [TransactionScopeAspect]
        public IResult Update(ProductSaveDto product, int productId)
        {
            product.IngredientNames = NormalizeIngredientNames(product.IngredientNames);
            product.AllergenIds = NormalizeAllergenIds(product.AllergenIds);

            var results = BusinessRules.Run(
                CheckIfProductNameExistsForUpdate(productId, product.ProductName));

            if (results != null)
            {
                return results;
            }

            var tenantSlug = _httpContextAccessor.HttpContext?.Items["TenantSlug"]?.ToString();
            if (string.IsNullOrWhiteSpace(tenantSlug))
            {
                return new ErrorResult("Tenant bulunamadi");
            }

            var oldProduct = _productDal.Get(p => p.ProductId == productId);
            if (oldProduct == null)
            {
                return new ErrorResult("Urun bulunamadi");
            }

            var newImage = NormalizeImage(product.Image);
            if (newImage == "nophoto.jpg" && !string.IsNullOrWhiteSpace(oldProduct.Image) && !IsCommonImage(oldProduct.Image))
            {
                _r2StorageService.DeleteProductImageAsync(tenantSlug, oldProduct.Image)
                    .GetAwaiter()
                    .GetResult();
            }
            else if (!string.IsNullOrWhiteSpace(newImage)
                     && !string.Equals(newImage, oldProduct.Image, StringComparison.OrdinalIgnoreCase)
                     && !string.IsNullOrWhiteSpace(oldProduct.Image)
                     && !IsCommonImage(oldProduct.Image))
            {
                _r2StorageService.DeleteProductImageAsync(tenantSlug, oldProduct.Image)
                    .GetAwaiter()
                    .GetResult();
            }
            else if (string.IsNullOrWhiteSpace(newImage))
            {
                newImage = oldProduct.Image;
            }

            product.Image = newImage;

            var result = _productDal.GetAll(p => p.CategoryId == product.CategoryId && p.ProductId != productId).Count;
            if (result >= 1000)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }

            var updated = _productDal.UpdateWithRelations(productId, product);
            if (!updated)
            {
                return new ErrorResult("Urun bulunamadi");
            }

            return new SuccessResult(Messages.ProductUpdated);
        }

        private IResult CheckIfProductCountOfCategoryError(int categoryId)
        {
            var result = _productDal.GetAll(p => p.CategoryId == categoryId).Count;
            if (result >= 15)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }

            return new SuccessResult();
        }

        private IResult CheckIfProductNameExists(string productName)
        {
            var result = _productDal.GetAll(p => p.ProductName == productName).Any();
            if (result)
            {
                return new ErrorResult(Messages.ProductNameAlreadyExists);
            }

            return new SuccessResult();
        }

        private IResult CheckIfProductNameExistsForUpdate(int productId, string productName)
        {
            var result = _productDal.Get(p =>
                p.ProductName == productName &&
                p.ProductId != productId);

            if (result != null)
            {
                return new ErrorResult("Bu isimde baska bir urun bulunmaktadir.");
            }

            return new SuccessResult();
        }

        [TransactionScopeAspect]
        public IResult AddTransactionalTest(Product product)
        {
            _productDal.Update(product);
            _productDal.Add(product);
            return new SuccessResult("Transaction Basarili - Urun guncelleme");
        }

        [CacheAspect]
        public IDataResult<List<ProductDto>> GetByCategoryName(string categoryName)
        {
            return new SuccessDataResult<List<ProductDto>>(_productDal.GetProductDtosByCategoryName(categoryName));
        }

        public IDataResult<List<Product>> ProductSearch(string name)
        {
            return new DataResult<List<Product>>(
                _productDal.GetAll(x => EF.Functions.Like(x.ProductName, $"%{name}%")),
                true,
                Messages.ProductListed);
        }

        [CacheAspect]
        public IDataResult<List<Product>> GetFeaturedProduct()
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(a => a.IsFeatured));
        }

        private static string NormalizeImage(string? image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "nophoto.jpg";
            }

            return image.Trim();
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

        private static bool IsCommonImage(string imageFileName)
        {
            return string.Equals(imageFileName, "nophoto.jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(imageFileName, "Quattro-logo.png", StringComparison.OrdinalIgnoreCase);
        }
    }
}
