using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
   public class AllergenDto  :IDto     // (Checkbox listesi ve ProductDto içinde kullanılacak)
    {
        public int AllergenId { get; set; }

        public string Name { get; set; } = null!;

        public string Icon { get; set; } = null!;
    }
}
