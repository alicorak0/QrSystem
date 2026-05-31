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

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Core.Utilities.Helpers;


namespace WebAPI.Controllers
{
    [Route("api/{tenant}/[controller]")] //Api End Point'i yani  insanlar api/controller yazacak URL kısmına 
    [ApiController]  // Attribute olmalı Controller için
    public class UploadController : ControllerBase
    {


        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            // 1️⃣ Boş mu?
            if (image == null || image.Length == 0)
                return BadRequest("Dosya yok.");


// 2️⃣ Boyut limiti (max 5MB)
if (image.Length > 5 * 1024 * 1024)
                return BadRequest("Dosya çok büyük (max 5MB).");

            // 3️⃣ Sadece izin verilen tipler
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(image.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                return BadRequest("Sadece jpg/png formatları izinli.");

            // 4️⃣ Klasör yolu
            var tenantSlug = HttpContext.Items["TenantSlug"]?.ToString();

            if (string.IsNullOrEmpty(tenantSlug))
                return BadRequest("Tenant bulunamadı");

            // 🔥 KLASÖR
            var folder = Path.Combine(
                _env.WebRootPath,
                "uploads",
                tenantSlug,
                "products"
            );



            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // 🔥 HER ZAMAN WEBP OLACAK
            //var fileName = Guid.NewGuid() + ".webp";
            var fileName= FileNameHelper.Generate(image.FileName); // Orijinal isimden temizlenmiş ve benzersiz bir isim oluştur
            var filePath = Path.Combine(folder, fileName);

            // 🔥 BURASI DEĞİŞTİ (kopyalama yerine dönüşüm)
            using (var stream = image.OpenReadStream())
            using (var img = await Image.LoadAsync(stream))
            {
                await img.SaveAsync(filePath, new WebpEncoder
                {
                    Quality = 75
                });
            }

            // 7️⃣ Dönüş
            return Ok(new
            {
                fileName,
                url = fileName
            });


}




    }
}
