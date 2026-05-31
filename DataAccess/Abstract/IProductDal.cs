using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Concrete;
using Core.DataAccess;
using Entities.DTOs;
using Core.Utilities.Results;
using System.Linq.Expressions;

namespace DataAccess.Abstract
{                      // Dal = Data access layer
    public  interface IProductDal : IEntityRepository<Product>  // Veriyi çekltiğimiz kısım class'a eşlik eden interfacemiz
    {
        //Daha kolay yolu Entity Repo kullanımı

        List<ProductDetailDto> GetProductDetails();
        
        public  List<Product> GetByCategoryName(string categoryName);

    }
}
