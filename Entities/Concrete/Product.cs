using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
   public class Product : IEntity// public diğer katanların erşimine açar
    {
        [Key]
        public int ProductId { get; set; }  
        public int CategoryId { get; set; } 

        public string ProductName { get; set; }

        public string? Description { get; set; }

        public string? Tooltip { get; set; }


        public int Price { get; set; }

        public string? Image { get; set; }

        //Favroilerde mi
        public bool IsFeatured { get; set; } = false; // öntanımlı olarak false

        //alerjen ve içerik 

        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        public ICollection<ProductAllergen> ProductAllergens { get; set; } = new List<ProductAllergen>();


    }
}
