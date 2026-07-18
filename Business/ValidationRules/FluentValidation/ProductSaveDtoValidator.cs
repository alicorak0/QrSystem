using Entities.DTOs;
using FluentValidation;

namespace Business.ValidationRules.FluentValidation
{
    public class ProductSaveDtoValidator : AbstractValidator<ProductSaveDto>
    {
        public ProductSaveDtoValidator()
        {
            RuleFor(p => p.ProductName)
                .NotEmpty()
                .WithMessage("Urun adi bos olamaz.")
                .MinimumLength(2)
                .WithMessage("Urun adi en az 2 karakter olmalidir.")
                .MaximumLength(180)
                .WithMessage("Urun adi en fazla 180 karakter olabilir.");

            RuleFor(p => p.Description)
                .MaximumLength(360)
                .WithMessage("Urun aciklamasi en fazla 360 karakter olabilir.");

            RuleFor(p => p.Price)
                .GreaterThan(0)
                .WithMessage("Urun fiyati 0'dan buyuk olmalidir.");

            RuleFor(p => p.CategoryId)
                .GreaterThan(0)
                .WithMessage("Lutfen bir kategori seciniz.");

            RuleFor(p => p.ProductName)
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ0-9\s\-\&\(\)\.]+$")
                .WithMessage("Urun adi gecersiz karakter iceriyor.");

            RuleForEach(p => p.IngredientNames)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Icerik adlari bos olamaz.");

            RuleForEach(p => p.AllergenIds)
                .GreaterThan(0)
                .WithMessage("Alerjen ID degeri gecersiz.");
        }
    }
}
