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
   public class MasterUserManager:IMasterUserService
    {
        IMasterUserDal _masterUserDal;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MasterUserManager(IMasterUserDal masterUserDal, IHttpContextAccessor httpContextAccessor)
        {
            _masterUserDal = masterUserDal;
            _httpContextAccessor = httpContextAccessor;

        }

        public List<OperationClaim> GetClaims(User user)
        {
            return _masterUserDal.GetClaims(user);
        }

        public void Add(User user)
        {
            _masterUserDal.Add(user);
        }

        public User GetByMail(string email)
        {


            return _masterUserDal.Get(x => x.Email == email);

        }

        public User GetByİd(int id)
        {
            return _masterUserDal.Get(u => u.Id == id);

        }
    }
}

