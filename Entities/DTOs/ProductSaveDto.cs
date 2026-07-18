using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
   public class ProductSaveDto : IDto  // FOR (Create & Update) API'nin Angular'a göndereceği ürün modeli.
    {
        public int CategoryId { get; set; }

        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }

        public string? Tooltip { get; set; }

        public int Price { get; set; }

        public string? Image { get; set; }

        public bool IsFeatured { get; set; }

        public List<string> IngredientNames { get; set; } = new();

        public List<int> AllergenIds { get; set; } = new();

    }
}
