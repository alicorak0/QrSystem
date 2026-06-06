using Business.Abstract;
using Business.Constants;
using Core.Aspects.Autofac.Caching;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CategoryManager : ICategoryService
    {

        ICategoryDal _categoryDal;

        public CategoryManager(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }

        [CacheRemoveAspect("ICategoryService.Get")]
        public IResult Add(Category category)
        {

            IResult results = BusinessRules.Run(CheckIfCategoryNameExists(category.CategoryName));

            //kural çalıştır

            if (results != null)  //result null dönerse işlemler devam eder Motora bak!
            {
                return results;
            }


            _categoryDal.Add(category);   //Entity Repo ile bağlantı DAL'daki

            return new SuccessResult(Messages.CategoryAdded);  //Result IResulttan türedi  sorun yok
        }

        [CacheRemoveAspect("ICategoryService.Get")]
        public IResult DeleteById(int id)
        {
            var categoryToDelete = _categoryDal.Get(p => p.CategoryId == id);
            if (categoryToDelete == null)
                return new ErrorResult("Category bulunamadı");

            try
            {
                _categoryDal.Delete(categoryToDelete);
                return new SuccessResult("Category silindi");
            }
            catch (DbUpdateException ex)
            {
                var fullException = ex.ToString() + " | " + (ex.InnerException?.ToString() ?? "");
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                
                // Foreign key / constraint hatasını yakala (Products tablosu bağlantısı)
                if (fullException.Contains("constraint", StringComparison.OrdinalIgnoreCase)
                    || fullException.Contains("FK_", StringComparison.OrdinalIgnoreCase)
                    || innerMessage.Contains("conflicted with", StringComparison.OrdinalIgnoreCase)
                    || innerMessage.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
                {
                    return new ErrorResult("Bu kategoriye bağlı ürünler bulunduğu için silme işlemi yapılamıyor. Lütfen önce kategoriye ait ürünleri silin");
                }

                return new ErrorResult("Kategori silinirken bir hata oluştu: " + innerMessage);
            }
            catch (Exception ex)
            {
                return new ErrorResult("Kategori silinirken beklenmeyen bir hata oluştu: " + ex.Message);
            }
        }



        [CacheAspect]
        public IDataResult<List<Category>> GetAll()
        {
            //İş Kodları
            return new SuccessDataResult<List<Category>>(_categoryDal.GetAll());   
        }

        [CacheAspect]
        public  IDataResult<Category> GetById(int categoryId)
        {
            return new SuccessDataResult<Category>(_categoryDal.Get(c=> c.CategoryId == categoryId));   
        }


        [CacheRemoveAspect("ICategoryService.Get")]
        public IResult Update(Category category)
        {
            _categoryDal.Update(category);
            return new SuccessResult(Messages.CategoryUpdated);
        }


        private IResult CheckIfCategoryNameExists(string categoryName) // hangi kategori istemniyor o gelmeli
        {
            var result = _categoryDal.GetAll(c => c.CategoryName == categoryName).Any(); // yeni dizini countu yani
            if (result)
            {
                return new ErrorResult(Messages.CategoryAlreadyExists);
            }

            return new SuccessResult();

        }






    }
}
