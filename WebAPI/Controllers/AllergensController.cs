using Business.Abstract;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/{tenant}/[controller]")]
    public class AllergensController : ApiControllerBase
    {
        private readonly IAllergenService _allergenService;

        public AllergensController(IAllergenService allergenService)
        {
            _allergenService = allergenService;
        }

        [HttpGet("getall")]
        [EnableRateLimiting("high")]
        public IActionResult GetAll()
        {
            var result = _allergenService.GetAll();
            return FromDataResult(result);
        }
    }
}
