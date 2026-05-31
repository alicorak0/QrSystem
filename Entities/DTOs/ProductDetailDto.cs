using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
  public  class ProductDetailDto:IDto
    {
         // Product ve Category alanları karışık olacak Joinle birleştiricem

        public int ProductId { get; set; } //Product Table
        public string ProductName { get; set; } //Product Table
        public string CategoryName { get; set; } //Category Table   

        public int Price { get; set; }

        public string Description { get; set; } 

        public string Tooltip { get; set; }

        public string İmage {  get; set; }


    }
}
