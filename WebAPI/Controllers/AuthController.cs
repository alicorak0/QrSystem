using Business.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebAPI.Security;

namespace WebAPI.Controllers
{
    [Route("api/{tenant}/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private IAuthService _authService;
        private  IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;

        }

        [EnableRateLimiting("low")]
        [HttpPost("login")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = _authService.Login(userForLoginDto);

            // 🔥 DEBUG BURAYA
            Console.WriteLine("LOGIN RESULT: " + userToLogin.Message);

            if (!userToLogin.Success)
                return Error(userToLogin.Message);

            var result = _authService.CreateAccessToken(userToLogin.Data);
            if (!result.Success)
                return Error(result.Message);

            // JWT cookie olarak ekle
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,               // JS erişemez
                Secure = true,                // Prod ortamda true yap
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(60)
            };
            Response.Cookies.Append("access_token", result.Data.Token, cookieOptions);

            return Success(data: null, message: "Login basarili");
        }


        [Authorize(Roles = "superadmin")]
        [EnableRateLimiting("low")]
        [HttpPost("register")]
        public IActionResult Register(UserForRegisterDto userForRegisterDto)
        {
            var userExists = _authService.UserExists(userForRegisterDto.Email);
            if (!userExists.Success)
                return Error(userExists.Message);

            // 🔥 TenantId middleware’den al
            var tenantIdObj = HttpContext.Items["TenantId"];

            if (tenantIdObj == null)
                return Error("Tenant bulunamadi");

            int tenantId = (int)tenantIdObj;

            // 🔥 Business'a gönder
            var registerResult = _authService.Register(
                userForRegisterDto,
                userForRegisterDto.Password,
                tenantId
            );

            if (!registerResult.Success)
                return Error(registerResult.Message);

            var result = _authService.CreateAccessToken(registerResult.Data);
            if (!result.Success)
                return Error(result.Message);

            // 🔥 COOKIE
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            Response.Cookies.Append("access_token", result.Data.Token, cookieOptions);

            return Success(data: null, message: "Kayit basarili");
        }

        //current user
        [Authorize]
        [Authorize(Policy = "TenantMatch")]
        [EnableRateLimiting("medium")]
        [HttpGet("me")]
        public IActionResult Me()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Error("Yetkisiz erisim.", StatusCodes.Status401Unauthorized);

            // ClaimTypes.NameIdentifier kullan
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Error("Yetkisiz erisim.", StatusCodes.Status401Unauthorized);

            var user = _userService.GetByİd(int.Parse(userIdClaim));
            if (user == null)
                return Error("Kullanici bulunamadi.", StatusCodes.Status401Unauthorized);

            // Frontend’e dönecek bilgiyi seçiyoruz
            return Success(new
            {
                fullName = user.FirstName + " " + user.LastName,
                email = user.Email,
                status = user.Status
            }, "Kullanici bilgisi getirildi.");
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // access_token cookie'sini sil
            if (Request.Cookies.ContainsKey("access_token"))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Prod ortamda true, dev ortamda false olabilir
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(-1) // geçmiş tarih vererek sil
                };

                Response.Cookies.Append("access_token", "", cookieOptions);
            }

            return Success(data: null, message: "Logout basarili");
        }





    }
}
