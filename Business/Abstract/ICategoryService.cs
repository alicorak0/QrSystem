using Core.Utilities.Results;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
   public interface ICategoryService
    {
        IDataResult<List<Category>> GetAll();
        IDataResult<Category> GetById(int categoryId);

        public IResult Add(Category category);   //Voidi IResult tipinde dönsün istiyorum

        public IResult Update(Category category);   //Voidi IResult tipinde dönsün istiyorum

        public IResult DeleteById(int id);
    }
}
