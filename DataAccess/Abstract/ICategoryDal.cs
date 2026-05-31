using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DataAccess;
using Core.Entities;
using Entities.Concrete;



namespace DataAccess.Abstract
{
   public interface ICategoryDal: IEntityRepository<Category>
    {
        //List<Category> GetAll();

        //void Add(Category category);
        //void Update(Category category);

        //void Delete(Category category);

        //List<Product> GetAllByCategory(int categoryId);
    }
}
