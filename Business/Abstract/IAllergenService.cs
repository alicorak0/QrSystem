using Core.Utilities.Results;
using Entities.DTOs;

namespace Business.Abstract
{
    public interface IAllergenService
    {
        IDataResult<List<AllergenDto>> GetAll();
    }
}
