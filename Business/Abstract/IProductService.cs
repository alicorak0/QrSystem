using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract  // Veriyi işleme classı olacak
{
  public  interface IProductService
    {
                 // Frontentin isteyebileceği şeyler brauda yer alacak servis aracları gibi düşün
        IDataResult<List<ProductDto>> GetAll(); // dikkat tipini list içiereisnde olrak gönderdim    
        IDataResult<List<ProductDto>> GetAllByCategory(int id);

        public IDataResult<List<ProductDto>> GetByCategoryName(string categoryName);

        IDataResult<List<ProductDetailDto>> GetProductDetails();

        IDataResult<Product> GetById(int id); // DALdaki elemanlara erişim yapcam bu ınterface kalıtım alıp class içinde yapacam

        public IResult Add(ProductSaveDto product);   //Voidi IResult tipinde dönsün istiyorum

        public IResult Update (ProductSaveDto product, int productId);   //Voidi IResult tipinde dönsün istiyorum

        public IResult AddTransactionalTest(Product product); //Uygulamalarda tutarlılıgı korumak

        public IResult Delete(int id);   //Sil

        public IDataResult<List<Product>> ProductSearch(string name);

        public IDataResult<List<Product>> GetFeaturedProduct();

    }
}
