using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
   public class ProductAllergen
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int AllergenId { get; set; }

        public Allergen Allergen { get; set; } = null!;

    }
}
