using Core.DataAccess;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataAccess.EntityFramework
{
   public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
      where TEntity : class, IEntity, new()
      where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext> _contextFactory;

        public EfEntityRepositoryBase(IDbContextFactory<TContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        protected TContext CreateContext()
        {
            return _contextFactory.CreateDbContext();
        }

        public void Add(TEntity entity)
        {
            using var context = CreateContext();
            var addedEntity = context.Entry(entity);
            addedEntity.State = Microsoft.EntityFrameworkCore.EntityState.Added;
            context.SaveChanges();
        }

        public void Delete(TEntity entity)
        {
            using var context = CreateContext();
            var deletedEntity = context.Entry(entity);
            deletedEntity.State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            context.SaveChanges();
        }

        public TEntity Get(Expression<Func<TEntity, bool>> filter)
        {
            using var context = CreateContext();
            return context.Set<TEntity>().SingleOrDefault(filter);
        }

        public List<TEntity> GetAll(Expression<Func<TEntity, bool>> filter = null)
        {
            using var context = CreateContext();
            return filter == null ? context.Set<TEntity>().ToList() : context.Set<TEntity>().Where(filter).ToList();
        }

        public List<TEntity> GetAllByCategory(int categoryId)
        {
            throw new NotImplementedException();
        }

        public void Update(TEntity entity)
        {
            using var context = CreateContext();
            var updatedEntity = context.Entry(entity);
            updatedEntity.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
        }
    }
}
