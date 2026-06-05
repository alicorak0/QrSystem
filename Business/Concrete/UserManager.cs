using Business.Abstract;
using Core.Entities.Concrete;
using DataAccess.Abstract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Business.Concrete
{
  public class UserManager:IUserService
    {
        IUserDal _userDal;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserManager(IUserDal userDal, IHttpContextAccessor httpContextAccessor)
        {
            _userDal = userDal;
            _httpContextAccessor = httpContextAccessor;

        }

        public List<OperationClaim> GetClaims(User user)
        {
            return _userDal.GetClaims(user);
        }

        public void Add(User user)
        {
            _userDal.Add(user);
        }

        public User GetByMail(string email)
        {
            var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"];

            if (tenantId == null)
            {
                return _userDal.Get(u => u.Email == email);
            }

            return _userDal.Get(u =>
                u.Email == email &&
                u.TenantId == (int)tenantId);
        }

        public User GetByİd(int id)
        {
            return _userDal.Get(u => u.Id == id);

        }
    }
}
