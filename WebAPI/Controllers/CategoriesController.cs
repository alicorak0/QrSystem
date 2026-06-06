using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Core.Utilities.Results;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using WebAPI.Security;

namespace WebAPI.Controllers
{
    [Route("api/{tenant}/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        ICategoryService _categoryService;
        private IHubContext<MenuHub> _hubContext;

        public CategoriesController(ICategoryService categoryService, IHubContext<MenuHub> hubContext) 
        { 
            _categoryService = categoryService;
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

        //[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
        [EnableRateLimiting("medium")]
        [HttpGet("getall")]
        public IActionResult GetAll()
        {

            var result = _categoryService.GetAll();
            if (result.Success)
            {
                return Ok(result); // data döndürdüm  
            }

            return BadRequest(result);

        }


        [Authorize(Roles = "admin")]
        [EnableRateLimiting("low")]
        [TenantMatch] // 🔥 Sadece kendi tenant'ına erişebilir
        [HttpPost("add")]
        public async Task<IActionResult> Add(Category category)
        {
            var result = _categoryService.Add(category);
            if (result.Success)
            {
                await NotifyTenantMenuUpdated();

                return Ok(result);

            }
            return BadRequest(new { message = result.Message });
        }

        [Authorize(Roles = "admin")]
        [EnableRateLimiting("low")]
        [TenantMatch] // 🔥 Sadece kendi tenant'ına erişebilir
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = _categoryService.DeleteById(id);

                if (result.Success)
                {
                    await NotifyTenantMenuUpdated();

                    return Ok(result);

                }

                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Silme işlemi sırasında beklenmeyen bir hata oluştu: " + ex.Message });
            }
        }


        [Authorize(Roles = "admin")]
        [EnableRateLimiting("low")]
        [TenantMatch] // 🔥 Sadece kendi tenant'ına erişebilir
        [HttpPost("update")]
        public async Task<IActionResult> Update(Category category)
        {
            var result = _categoryService.Update(category);

            if (result.Success)
            {
                await NotifyTenantMenuUpdated();

                return Ok(result);
            }

            return BadRequest(new { message = result.Message });
        }





    }
}
