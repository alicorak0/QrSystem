using Business.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class MasterAuthController : ControllerBase
    {
        private readonly IMasterAuthService _masterAuthService;

        public MasterAuthController(IMasterAuthService masterAuthService)
        {
            _masterAuthService = masterAuthService;
        }

        [HttpPost("login")]
        public IActionResult Login(UserForLoginDto dto)
        {
            var result = _masterAuthService.Login(dto); // 👈 MASTER DB

            if (!result.Success)
                return BadRequest(result.Message);

            var token = _masterAuthService.CreateAccessToken(result.Data);

            return Ok(new { token = token.Data.Token });
        }

        // 👤 REGISTER (SADECE USER CREATE)
        [HttpPost("register")]
        public IActionResult Register(UserForRegisterDto dto)
        {
            var result = _masterAuthService.Register(dto, dto.Password);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new
            {
                message = "Kullanıcı oluşturuldu",
                userId = result.Data.Id
            });
        }
    }
}
