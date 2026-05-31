using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Constants
{
    public static class Messages
    { // erişmek nesneden bağımsız olsun sadece text alıyorum 

        public static string ProductAdded = "Ürün Eklendi";
        public static string ProductUpdated = "Ürün Güncellendi";
        public static string ProductNameInvalid = "Ürün İsmi Geçersiz";
        public static string MaintanenceTime = "Bakım zamanı Lütfen Daha Sonra Tekrar Deneyiniz";
        public static string ProductListed = "Ürünler Listelendi";
        public static string UnitPriceInvalid = "Geçersiz Birim Fiyat Girişi";
        public static string ProductCountOfCategoryError = "Aynı Kategoride 10 adet ürün bulunamaz!";
        public static string ProductNameAlreadyExists = "Bu isimde zaten bir ürün mevcut";
        public static string CategoryLimitExceded = "Category Sınırı Aşıldı!";
        public static string AuthorizationDenied = "Yetkiniz yok";
        public static string AuthenticationError = "Geçersiz Kullanıcı !";
        public static string CategoryAdded = "Category Eklendi";
        public static string CategoryUpdated = "Category Güncellendi";
        public static string CategoryAlreadyExists = "Bu isimde zaten bir kategori mevcut";

    }
}
