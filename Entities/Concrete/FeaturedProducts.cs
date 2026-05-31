using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
  public  class FeaturedProducts:IEntity
    {


        [Key]
        public int ProductId { get; set; }
        public int CategoryId { get; set; }

        public string ProductName { get; set; }

        public string Description { get; set; }

        public string Tooltip { get; set; }


        public int Price { get; set; }

        public string Image { get; set; }
    }
}
