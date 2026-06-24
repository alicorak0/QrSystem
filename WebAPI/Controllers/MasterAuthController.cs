using Business.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/auth")]
    public class MasterAuthController : ApiControllerBase
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
                return Error(result.Message);

            var token = _masterAuthService.CreateAccessToken(result.Data);
            if (!token.Success)
                return Error(token.Message);

            return Success(new { token = token.Data.Token }, token.Message);
        }

        // 👤 REGISTER (SADECE USER CREATE)
        [HttpPost("register")]
        public IActionResult Register(UserForRegisterDto dto)
        {
            var result = _masterAuthService.Register(dto, dto.Password);

            if (!result.Success)
                return Error(result.Message);

            return Success(new
            {
                userId = result.Data.Id
            }, "Kullanici olusturuldu");
        }
    }
}
