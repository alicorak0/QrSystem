using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Core.Utilities.Results;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;

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
        [HttpPost("add")]
        public async Task<IActionResult> Add(Category category)
        {
            var result = _categoryService.Add(category);
            if (result.Success)
            {
                // return Ok(new { message = "ali" });
                await _hubContext.Clients.All.SendAsync("MenuUpdated");

                return Ok(result);

            }
            return BadRequest(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = _categoryService.DeleteById(id);

            if (result.Success)
            {
                await _hubContext.Clients.All.SendAsync("MenuUpdated");

                return Ok(result);

            }

            return BadRequest(result);
        }


        [Authorize(Roles = "admin")]
        [HttpPost("update")]
        public async Task<IActionResult> Update(Category category)
        {
            var result = _categoryService.Update(category);

            if (result.Success)
            {
                await _hubContext.Clients.All.SendAsync("MenuUpdated");

                return Ok(result);
            }

            return BadRequest(result);
        }





    }
}
