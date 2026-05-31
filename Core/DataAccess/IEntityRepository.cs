using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.DataAccess
{                                                //Kısıtlama ile Ientity veya IEntity den türeyen class kabul eder
   public interface IEntityRepository<T> where T :class,IEntity,new()  // generic yapıcaz ki Product category customer tutablisn hepsini
    {

        void Add(T entity);
        void Update(T entity);

        void Delete(T entity);

        List<T> GetAllByCategory(int categoryId); // daha bağımsız sorgu tekniği olan kod aşağıda

        List<T> GetAll(Expression<Func<T, bool>> filter=null); // filtre de vermeyeblirsin

        T Get(Expression<Func<T, bool>> filter); // tek bir entity detayı getirmek için
        // buraya null gönderemezsin





    }
}
