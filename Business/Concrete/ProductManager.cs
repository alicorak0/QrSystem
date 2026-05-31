    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.CrossCuttingConcern.Validataion;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.InMemory;
using Entities.Concrete;
using Entities.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Core.Aspects.Autofac.Validation;
using Business.CCS;
using Core.Utilities.Business;
using Business.BusinessAspects.Autofac;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.MyIntereceptor;
using Microsoft.Extensions.Caching.Memory;
using Core.Utilities.IoC;
using Microsoft.AspNetCore.Http;

namespace Business.Concrete
{
     // Business sadece gelen veriyi bilir nasıl geldiğini değil
    public class ProductManager : IProductService
    {
        //    InMemoryProductDal _InMemoryProductDal; Bağımlı hale getirir yapma ! Soyutlama ile bilgi alacam

        IProductDal _productDal;
        ICategoryService _categoryService;
        private readonly IMemoryCache _memoryCache; // <-- inject edilen cache
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductManager(IProductDal productDal,ICategoryService categoryService, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor) // method injection // veri her  yerden gelebilir bağımsız memory or DB
        {
            _productDal = productDal;
            _categoryService = categoryService; // Category ilgilend,iren kural varsa servisini dahil ederim
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
        }

        //[ValidationAspect(typeof(ProductValidator))]

        [CacheRemoveAspect("IProductService.Get")]
        public IResult Add(Product product)
        {
            //Updatede bu şart var fakat 10 yerine 15 yapınca diğer kural etkilenmez ve aynı kalır

            IResult results=  BusinessRules.Run(CheckIfProductNameExists(product.ProductName),
                CheckIfProductCountOfCategoryError(product.CategoryId)
                );

            if(results != null )  //result null dönerse işlemler devam eder Motora bak!
            {
                return results; 
            }


            _productDal.Add(product);   //Entity Repo ile bağlantı DAL'daki


            //CACHE
            // Manuel cache temizleme
            //_memoryCache.Remove($"IProductService.GetAllByCategory({product.CategoryId})");

            return new SuccessResult(Messages.ProductAdded);  //Result IResulttan türedi  sorun yok



                
            //aynı isimde ürün eklenemez kotrnolu yap



            //ValidataionTool.Validate(new ProductValidator(), product);    Eski version

            //business code yer alacak burada

            //Girişteki Loggeri çalıştır
            //_logger.Log();
            //try
            //{
            //    _productDal.Add(product);   //Entity Repo ile bağlantı DAL'daki

            //    return new SuccessResult(Messages.ProductAdded);  //Result IResulttan türedi  sorun yok

            //}
            //catch (Exception exception) 
            //{
            //    _logger.Log();
            //}

            //return new ErrorResult();




            // return new SuccessResult("Ürün başarıyla eklendi");  Mesaj verilmez sadece true döner


        }

        [CacheAspect] /// key ,value
        public IDataResult<List<Product>> GetAll()
        {
            

            
            return new DataResult<List<Product>>(_productDal.GetAll(), true, Messages.ProductListed);
        }

        [CacheAspect]
        public IDataResult<List<Product>> GetAllByCategory(int id)
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(x=>x.CategoryId == id));   
        }

         [CacheAspect]
        [PerformanceAspect(2)] // 5 saniyeyi geçerse uyar   
        public IDataResult<Product> GetById(int id)
        {
          //  System.Threading.Thread.Sleep(5000); // 3 saniye
            return new SuccessDataResult<Product>(_productDal.Get(p=>p.ProductId == id));
        }


        [CacheRemoveAspect("IProductService.Get")]
        public IResult Delete(int id)
        {
            var productToDelete = _productDal.Get(p => p.ProductId == id);
            if (productToDelete == null)
                return new ErrorResult("Ürün bulunamadı");

            _productDal.Delete(productToDelete);
            return new SuccessResult("Ürün silindi");
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

        //[CacheRemoveAspect("IProductService.Get")]
        //[ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public IResult Update(Product product)
        {
            //dosya adını tenanta göre al

            var tenantSlug = _httpContextAccessor.HttpContext?
    .Items["TenantSlug"]?.ToString();

            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                tenantSlug,
                "products"
            );



            var oldProduct = _productDal.Get(p => p.ProductId == product.ProductId);

            if (oldProduct == null)
                return new ErrorResult("Ürün bulunamadı");

            // ----------------------------
            // 1) FOTO SİLME DURUMU
            // ----------------------------
            if (product.Image == "noPhoto.jpg" && oldProduct.Image != "noPhoto.jpg")
            {
                var oldPath = Path.Combine(folder, oldProduct.Image);

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                product.Image = "noPhoto.jpg";
            }

            // ----------------------------
            // 2) YENİ FOTO YÜKLENDİYSE
            // ----------------------------
            else if (!string.IsNullOrEmpty(product.Image) && product.Image != oldProduct.Image)
            {
                if (oldProduct.Image != "noPhoto.jpg")
                {
                    var oldPath = Path.Combine(folder, oldProduct.Image);

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                product.Image = product.Image;
            }
            else
            {
                // foto değişmediyse eskiyi koru
                product.Image = oldProduct.Image;
            }

            // ----------------------------
            // 3) CATEGORY LIMIT KONTROL
            // ----------------------------
            var result = _productDal.GetAll(p => p.CategoryId == product.CategoryId).Count;

            if (result >= 1000)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }

            // ----------------------------
            // 4) UPDATE
            // ----------------------------
            _productDal.Update(product);

            return new SuccessResult(Messages.ProductUpdated);
        }

        //sadece bu snıfta kullanıalcak check metodu yazılacak public yapmıyacaz

        private IResult CheckIfProductCountOfCategoryError(int categoryId) // hangi kategori istemniyor o gelmeli
        {
            var result = _productDal.GetAll(p => p.CategoryId == categoryId).Count; // yeni dizini countu yani
            if (result >= 15)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }

            return new SuccessResult();
         
        }

        private IResult CheckIfProductNameExists(string productName) // hangi kategori istemniyor o gelmeli
        {
            var result = _productDal.GetAll(p => p.ProductName == productName).Any(); // yeni dizini countu yani
            if (result)
            {
                return new ErrorResult(Messages.ProductNameAlreadyExists);
            }

            return new SuccessResult();

        }
        //private IResult CheckIfCategoryLimitExceded()
        //{
        //    var result = _categoryService.GetAll();

        //    if (result.Data.Count > 15)
        //    {
        //        return new ErrorResult(Messages.CategoryLimitExceded);
        //    }

        //    return new SuccessResult();
        //}

        [TransactionScopeAspect] //Transaction metot olarak işaretleme
        public IResult AddTransactionalTest(Product product)
        {
          _productDal.Update(product);
            _productDal.Add(product);
            return new SuccessResult("Transaction Başarılı -Ürün güncelleme");


        }
        [CacheAspect]
        public IDataResult<List<Product>> GetByCategoryName(string categoryName)
        {
          

  return new SuccessDataResult<List<Product>>(_productDal.GetByCategoryName(categoryName));

        }

        public IDataResult<List<Product>> ProductSearch(string name)
        {

            return new DataResult<List<Product>>(_productDal.GetAll(x=>EF.Functions.Like(x.ProductName, $"%{name}%")), true, Messages.ProductListed);

        }

        [CacheAspect]
        public IDataResult<List<Product>> GetFeaturedProduct()
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(a => a.IsFeatured == true));
        }
    }
}
