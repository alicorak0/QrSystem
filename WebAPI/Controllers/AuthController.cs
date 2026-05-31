using Business.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/{tenant}/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private IAuthService _authService;
        private  IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;

        }

        [HttpPost("login")]
        public ActionResult Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = _authService.Login(userForLoginDto);
            if (!userToLogin.Success)
                return BadRequest(userToLogin.Message);

            var result = _authService.CreateAccessToken(userToLogin.Data);
            if (!result.Success)
                return BadRequest(result.Message);

            // JWT cookie olarak ekle
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,               // JS erişemez
                Secure = true,                // Prod ortamda true yap
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(60)
            };
            Response.Cookies.Append("access_token", result.Data.Token, cookieOptions);

            return Ok(new { message = "Login başarılı" });
        }

        [HttpPost("register")]
        public ActionResult Register(UserForRegisterDto userForRegisterDto)
        {
            var userExists = _authService.UserExists(userForRegisterDto.Email);
            if (!userExists.Success)
                return BadRequest(userExists.Message);

            var registerResult = _authService.Register(userForRegisterDto, userForRegisterDto.Password);
            var result = _authService.CreateAccessToken(registerResult.Data);
            if (!result.Success)
                return BadRequest(result.Message);

            // JWT cookie olarak ekle
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            Response.Cookies.Append("access_token", result.Data.Token, cookieOptions);

            return Ok(new { message = "Kayıt başarılı" });
        }

        //current user
        [HttpGet("me")]
        public IActionResult Me()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            // ClaimTypes.NameIdentifier kullan
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var user = _userService.GetByİd(int.Parse(userIdClaim));
            if (user == null)
                return Unauthorized();

            // Frontend’e dönecek bilgiyi seçiyoruz
            return Ok(new
            {
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Status = user.Status
            });
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

            return Ok(new { message = "Logout başarılı" });
        }





    }
}
