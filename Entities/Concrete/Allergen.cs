using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Allergen : Core.Entities.IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Icon { get; set; } = null!;

        public ICollection<ProductAllergen> ProductAllergens { get; set; } = new List<ProductAllergen>();

    }
}
