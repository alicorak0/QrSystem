using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Utilities.Helpers
{
   public static class SlugHelper
    {

        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return "";

            string str = phrase.ToLowerInvariant();

            // Türkçe karakterleri normal harfe çevir
            str = str.Replace("ç", "c")
                     .Replace("ğ", "g")
                     .Replace("ı", "i")
                     .Replace("ö", "o")
                     .Replace("ş", "s")
                     .Replace("ü", "u");

            // Özel karakterleri kaldır
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // Boşlukları ve tireleri normalize et
            str = Regex.Replace(str, @"\s+", "-").Trim('-');

            // Ardışık tireleri tek tireye indir
            str = Regex.Replace(str, @"-+", "-");

            return str;
        }
    }
}
