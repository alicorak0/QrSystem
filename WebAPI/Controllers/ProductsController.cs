using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Business.Concrete;
using DataAccess.Concrete.EntityFramework;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Core.Aspects.Autofac.Caching;
using Business.BusinessAspects.Autofac;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;
using WebAPI.Security;
using Microsoft.AspNetCore.RateLimiting;

namespace WebAPI.Controllers
{
    [Route("api/{tenant}/[controller]")] //Api End Point'i yani  insanlar api/controller yazacak URL kısmına 
    public class ProductsController : ApiControllerBase
    {
       
        IProductService _productService;
        private  IHubContext<MenuHub> _hubContext;


        //AOP kullanıcaz tekrar eden bloklkar olmayacak
        public ProductsController(IProductService productService, IHubContext<MenuHub> hubContext) 
        {
            _productService = productService;       
            _hubContext = hubContext;
        }

        private Task NotifyTenantMenuUpdated()
        {
            var tenantSlug = HttpContext.Items["TenantSlug"]?.ToString();
            if (string.IsNullOrWhiteSpace(tenantSlug))
            {
                return Task.CompletedTask;
            }

            return _hubContext.Clients.Group(MenuHub.GetGroupName(tenantSlug)).SendAsync("MenuUpdated");
        }

        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("getall")]
        [EnableRateLimiting("high")]
        public IActionResult GetAll() 
        {

            //Thread.Sleep(5000);
            var result = _productService.GetAll();
            return FromDataResult(result);

        }


        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("getallfeaturedproduct")]
        [EnableRateLimiting("high")]
        public IActionResult GetAllFeaturedProduct()
        {

            //Thread.Sleep(5000);
            var result = _productService.GetFeaturedProduct();
            return FromDataResult(result);

        }


        //İd ile tek ürün getirelim
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("getbycategory")]
        [EnableRateLimiting("medium")]
        public IActionResult GetByCategory(int categoryId) 
        { 

                var result =_productService.GetAllByCategory(categoryId);
            return FromDataResult(result);
        }


        //İd ile tek ürün getirelim
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("getbycategoryname/{categoryName}")]
        [EnableRateLimiting("medium")]
        public IActionResult GetByCategory(string categoryName)
        {

            var result = _productService.GetByCategoryName(categoryName);
            return FromDataResult(result);
        }

        [Authorize(Roles = "admin")]
        [Authorize(Policy = "TenantMatch")] // 🔥 Sadece kendi tenant'ına erişebilir
        [HttpPost("add")]
        [EnableRateLimiting("low")]
        public async Task<IActionResult> Add(Product product)
        {
            var result = _productService.Add(product);
            if (result.Success)
            {
                await NotifyTenantMenuUpdated();

                return Success(data: null, message: result.Message);
                                      // test için yazıldı
                
            }
            return Error(result.Message);  
        }


     

        [Authorize(Roles = "admin,superadmin")]
        [Authorize(Policy = "TenantMatch")]
        [HttpPost("update")]
        [EnableRateLimiting("low")]
        public async Task<IActionResult> Update(Product product)
        {
            var result = _productService.Update(product);

            if (result.Success)
            {
                // ✅ Güncelleme event
                await NotifyTenantMenuUpdated();
                return Success(data: null, message: result.Message);
            }

            return Error(result.Message);
        }


        [Authorize(Roles = "admin")]
        [Authorize(Policy = "TenantMatch")]
        [HttpDelete("delete/{id}")]
        [EnableRateLimiting("low")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = _productService.Delete(id);

            if (result.Success)
            {
                await NotifyTenantMenuUpdated();

                return Success(data: null, message: result.Message);

            }

            return Error(result.Message);
        }


        // GET: api/products/search?name=abc
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("search")]
        [EnableRateLimiting("medium")]
        public IActionResult Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Success(new List<Product>(), "Lutfen bir urun adi girin");
            }

            var result = _productService.ProductSearch(name);

            if (result.Success)
            {
                if (result.Data.Count == 0)     // bOŞ LİSTE DÖNDÜR SONUÇ BULUNAMADI
                    return Success(result.Data, "Sonuc bulunamadi");

                return Success(result.Data, result.Message);
            }

            return Error(result.Message);
        }



    }
}
