using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Concrete;
using FluentValidation;

namespace Business.ValidationRules.FluentValidation
{
    // Prodct girilen bilghieri  doğrulayacak sınıf
   public class ProductValidator:AbstractValidator<Product>
    {
        public ProductValidator() 
        {
                 RuleFor(p => p.ProductName)
     .NotEmpty()
     .WithMessage("Ürün adı boş olamaz.");

      RuleFor(p => p.ProductName)
          .MinimumLength(2)
          .WithMessage("Ürün adı en az 2 karakter olmalıdır.");

      RuleFor(p => p.ProductName)
          .MaximumLength(40)
          .WithMessage("Ürün adı en fazla 40 karakter olabilir.");

      RuleFor(p => p.Description)
          .MaximumLength(360)
          .WithMessage("Ürün açıklaması en fazla 360 karakter olabilir.");

      RuleFor(p => p.Price)
          .NotNull()
          .WithMessage("Fiyat boş olamaz.");

      RuleFor(p => p.Price)
          .GreaterThan(0)
          .WithMessage("Ürün fiyatı 0'dan büyük olmalıdır.");

      RuleFor(p => p.CategoryId)
          .GreaterThan(0)
          .WithMessage("Lütfen bir kategori seçiniz.");

      RuleFor(p => p.ProductName)
          .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ0-9\s\-\&\(\)\.]+$")
          .WithMessage("Ürün adı geçersiz karakter içeriyor.");

      RuleFor(p => p.Description)
          .Must(x => x == null || !string.IsNullOrWhiteSpace(x))
          .WithMessage("Ürün açıklaması yalnızca boşluklardan oluşamaz.");
        }

        //private bool StartWithA(string arg)
        //{
        //    return arg.StartsWith("A");
        //}
    }
}
