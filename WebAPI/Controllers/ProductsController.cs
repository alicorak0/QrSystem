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

        [HttpGet("getall")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
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


        [HttpGet("getallfeaturedproduct")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
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
        [HttpGet("getbycategory")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
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
        [HttpGet("getbycategoryname/{categoryName}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
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
        [HttpPost("add")]       
        public async Task<IActionResult> Add(Product product)
        {
            var result = _productService.Add(product);
            if (result.Success)
            {

                await _hubContext.Clients.All.SendAsync("MenuUpdated");

                return Ok(result);
                                      // test için yazıldı
                //var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //return Ok(new
                //{
                //    message = $"Ürün eklendi. Ekleyen UserId: {userId}"
                    
                //});
            }
            return BadRequest(result);  
        }


        //[HttpGet]
        //public List<Product> Get()
        //{
        //    return new List<Product>
        //    {
        //        new Product{ProductId=1,ProductName="Elma"},
        //        new Product{ProductId=1,ProductName="Elma"}

        //    };
        //}








        //[HttpGet]  //            [HttpGet("text")]
        //public  string Get()
        //{

        //    return "Merhaba";
        //}


        ////[HttpGet("name")]
        ////public IActionResult GetAction()
        ////{
        ////    return Ok(new { Name = "Ali" });
        ////}


        [Authorize(Roles = "admin")]
        [HttpPost("update")]
        public async Task<IActionResult> Update(Product product)
        {
            var result = _productService.Update(product);

            if (result.Success)
            {
                // ✅ Güncelleme event
                await _hubContext.Clients.All.SendAsync("MenuUpdated");
                return Ok(result);
            }

            return BadRequest(result);
        }


        [Authorize(Roles = "admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = _productService.Delete(id);

            if (result.Success)
            {
                await _hubContext.Clients.All.SendAsync("MenuUpdated");

                return Ok(result);

            }

            return BadRequest(result);
        }


        // GET: api/products/search?name=abc
        [HttpGet("search")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
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
