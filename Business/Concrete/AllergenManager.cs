using Business.Abstract;
using Core.Aspects.Autofac.Caching;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.DTOs;

namespace Business.Concrete
{
    public class AllergenManager : IAllergenService
    {
        private readonly IAllergenDal _allergenDal;

        public AllergenManager(IAllergenDal allergenDal)
        {
            _allergenDal = allergenDal;
        }

        [CacheAspect]
        public IDataResult<List<AllergenDto>> GetAll()
        {
            return new SuccessDataResult<List<AllergenDto>>(_allergenDal.GetAllDtos());
        }
    }
}
