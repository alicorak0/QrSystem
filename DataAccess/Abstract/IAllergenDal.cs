using Core.DataAccess;
using Entities.Concrete;
using Entities.DTOs;

namespace DataAccess.Abstract
{
    public interface IAllergenDal : IEntityRepository<Allergen>
    {
        List<AllergenDto> GetAllDtos();
    }
}
