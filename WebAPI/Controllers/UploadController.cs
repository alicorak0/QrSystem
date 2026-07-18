using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using Core.Utilities.Helpers;


namespace WebAPI.Controllers
{
    [Route("api/{tenant}/[controller]")] //Api End Point'i yani  insanlar api/controller yazacak URL kısmına 
    public class UploadController : ApiControllerBase
    {


        private readonly IR2StorageService _r2StorageService;

        public UploadController(IR2StorageService r2StorageService)
        {
            _r2StorageService = r2StorageService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            // 1️⃣ Boş mu?
            if (image == null || image.Length == 0)
                return Error("Dosya yok.");


            // 2️⃣ Boyut limiti (max 5MB)
            if (image.Length > 5 * 1024 * 1024)
                return Error("Dosya cok buyuk (max 5MB).");

            // 3️⃣ Sadece izin verilen tipler
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(image.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                return Error("Sadece jpg/png formatlari izinli.");

            // 4️⃣ Klasör yolu
            var tenantSlug = HttpContext.Items["TenantSlug"]?.ToString();

            if (string.IsNullOrEmpty(tenantSlug))
                return Error("Tenant bulunamadi");

            // 🔥 HER ZAMAN WEBP OLACAK
            var fileName = FileNameHelper.Generate(image.FileName);

            // ESKI LOCAL DISK AKISI (R2 MIGRASYONU ICIN COMMENT OLARAK BIRAKILDI)
            // var folder = Path.Combine(_env.WebRootPath, "uploads", tenantSlug, "products");
            // if (!Directory.Exists(folder))
            //     Directory.CreateDirectory(folder);
            // var filePath = Path.Combine(folder, fileName);
            // using (var stream = image.OpenReadStream())
            // using (var img = await Image.LoadAsync(stream))
            // {
            //     await img.SaveAsync(filePath, new WebpEncoder { Quality = 75 });
            // }

            // Yeni akis: ImageSharp ile WebP donusturup stream'i R2'ye yukler.
            await using var output = new MemoryStream();
            using (var stream = image.OpenReadStream())
            using (var img = await Image.LoadAsync(stream))
            {
                await img.SaveAsync(output, new WebpEncoder
                {
                    Quality = 75
                });
            }

            output.Position = 0;

            var uploadResult = await _r2StorageService.UploadProductImageAsync(
                tenantSlug,
                fileName,
                output,
                "image/webp",
                HttpContext.RequestAborted);

            if (!uploadResult.Success)
            {
                return Error(uploadResult.Message);
            }

            // 7️⃣ Dönüş
            return Success(new
            {
                fileName,
                url = fileName
            }, "Dosya yuklendi.");
        }

        [AllowAnonymous]
        [HttpGet("/uploads/{tenant}/products/{fileName}")]
        public async Task<IActionResult> GetTenantProductImage(string tenant, string fileName)
        {
            var imageResult = await _r2StorageService.GetProductImageAsync(tenant, fileName, HttpContext.RequestAborted);

            if (!imageResult.Success)
            {
                return NotFound();
            }

            return File(imageResult.Data.Content, imageResult.Data.ContentType);
        }



    }
}
