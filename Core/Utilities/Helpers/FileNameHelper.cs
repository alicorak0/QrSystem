using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Core.Utilities.Helpers
{
    public static class FileNameHelper
    {
        public static string Generate(string originalFileName)
        {
            // 1️⃣ uzantıyı al (.jpg vs)
            var extension = Path.GetExtension(originalFileName);

            // 2️⃣ sadece isim kısmı
            var name = Path.GetFileNameWithoutExtension(originalFileName);

            // 3️⃣ slugify (temizleme)
            var safeName = Slugify(name);

            // 4️⃣ kısa guid
            var unique = Guid.NewGuid().ToString().Substring(0, 8);

            // 5️⃣ final (webp kullanıyorsan uzantıyı değiştir)
            return $"{safeName}_{unique}.webp";
        }

        private static string Slugify(string text)
        {
            text = text.ToLower();

            // Türkçe karakterleri düzelt
            text = text
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ç", "c")
                .Replace("ğ", "g");

            // boşluk → -
            text = text.Replace(" ", "-");

            // özel karakter temizle
            text = Regex.Replace(text, @"[^a-z0-9\-]", "");

            // fazla - temizle
            text = Regex.Replace(text, @"-+", "-");

            return text.Trim('-');
        }
    }
}
