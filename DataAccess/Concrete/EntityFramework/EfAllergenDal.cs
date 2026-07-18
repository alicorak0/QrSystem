using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfAllergenDal : EfEntityRepositoryBase<Allergen, QrMenuContext>, IAllergenDal
    {
        private readonly IDbContextFactory<QrMenuContext> _contextFactory;

        public EfAllergenDal(IDbContextFactory<QrMenuContext> contextFactory)
            : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<AllergenDto> GetAllDtos()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Allergens
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .Select(a => new AllergenDto
                {
                    AllergenId = a.Id,
                    Name = a.Name,
                    Icon = a.Icon
                })
                .ToList();
        }
    }
}
