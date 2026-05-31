using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.InMemory
{
   public class InMemoryProductDal : IProductDal
    {
        List<Product> _products; // bu global değişken için kullanılır  _ ile globaloldugunu belirt

        public InMemoryProductDal() 
        {
            ////Db den geldiğini düşünüp simile edelim
            //_products = new List<Product> {
            //    new Product { ProductId=1, CategoryId=1, ProductName="Bardak", UnitPrice=15, UnitsInStock=15},
            //    new Product { ProductId=2, CategoryId=1, ProductName="Kamera", UnitPrice=500, UnitsInStock=3},
            //    new Product { ProductId=3, CategoryId=2, ProductName="Telefon", UnitPrice=1500, UnitsInStock=2},
            //    new Product { ProductId=4, CategoryId=2, ProductName="Klavye", UnitPrice=150, UnitsInStock=65},
            //    new Product { ProductId=5, CategoryId=2, ProductName="Fare", UnitPrice=85, UnitsInStock=1}
            //    };
        }

        public void Add(Product product)
        {
            _products.Add(product);
        }
        
        public void Delete(Product product)
        {

            var deletedProduct = _products.SingleOrDefault(x=>x.ProductId == product.ProductId);
            
            _products.Remove(deletedProduct);

        }

        public List<Product> GetAll()
        {
            return _products;   
        }

        
        public void Update(Product product)
        {
            var updatedProduct = _products.SingleOrDefault(p=>p.ProductId == product.ProductId);

            updatedProduct.ProductName = product.ProductName;
            updatedProduct.CategoryId = product.CategoryId; 
        }


        public List<Product> GetAllByCategory(int categoryId)
        {
            
             var x = _products.Where(p=>p.CategoryId == categoryId).ToList();
              Console.WriteLine(x.GetType());
            return x;
        }

        public List<Product> GetAll(Expression<Func<Product, bool>> filter = null)
        {
            throw new NotImplementedException();
        }

        public Product Get(Expression<Func<Product, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public List<ProductDetailDto> GetProductDetails()
        {
            throw new NotImplementedException();
        }

        public List<Product> GetByCategoryName(string categoryName)
        {
            throw new NotImplementedException();
        }
    }
}
