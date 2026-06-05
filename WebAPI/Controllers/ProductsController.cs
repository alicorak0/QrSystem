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
using Core.Utilities.Results;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;
using WebAPI.Security;
using Microsoft.AspNetCore.RateLimiting;

namespace WebAPI.Controllers
{
    [Route("api/{tenant}/[controller]")] //Api End Point'i yani  insanlar api/controller yazacak URL kısmına 
    [ApiController]  // Attribute olmalı Controller için
    public class ProductsController : ControllerBase
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
            if (result.Success) 
            {
                return Ok(result); // data döndürdüm  
            }

            return BadRequest(result);

        }


        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("getallfeaturedproduct")]
        [EnableRateLimiting("high")]
        public IActionResult GetAllFeaturedProduct()
        {

            //Thread.Sleep(5000);
            var result = _productService.GetFeaturedProduct();
            if (result.Success)
            {
                return Ok(result); // data döndürdüm  
            }

            return BadRequest(result);

        }


        //İd ile tek ürün getirelim
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("getbycategory")]
        [EnableRateLimiting("medium")]
        public IActionResult GetByCategory(int categoryId) 
        { 

                var result =_productService.GetAllByCategory(categoryId);
            if (result.Success) 
            {
                return Ok(result);            
            }
              return BadRequest(result);
        }


        //İd ile tek ürün getirelim
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("getbycategoryname/{categoryName}")]
        [EnableRateLimiting("medium")]
        public IActionResult GetByCategory(string categoryName)
        {

            var result = _productService.GetByCategoryName(categoryName);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
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

                return Ok(result);
                                      // test için yazıldı
                
            }
            return BadRequest(result);  
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
                return Ok(result);
            }

            return BadRequest(result);
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

                return Ok(result);

            }

            return BadRequest(result);
        }


        // GET: api/products/search?name=abc
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [HttpGet("search")]
        [EnableRateLimiting("medium")]
        public IActionResult Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
return Ok(new DataResult<List<Product>>(
            new List<Product>(), // boş liste
            true,                // success
            "Lütfen bir ürün adı girin" // mesaj
        ));            }

            IDataResult<List<Product>> result = _productService.ProductSearch(name);

            if (result.Success)
            {
                if (result.Data.Count == 0)     // bOŞ LİSTE DÖNDÜR SONUÇ BULUNAMADI
                    return Ok(new DataResult<List<Product>>(
       result.Data, // boş liste
       true,        // success
       "Sonuç Bulunamadı" // mesaj
   ));

                return Ok(result); // 200 + Data + Message
            }

            return BadRequest(result.Message);
        }



    }
}
