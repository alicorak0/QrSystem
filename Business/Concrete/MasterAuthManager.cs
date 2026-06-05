using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.JWT;
using DataAccess.Concrete.EntityFramework;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
   public class MasterAuthManager:IMasterAuthService
    {
        private readonly MasterDbContext _context;
        private readonly ITokenHelper _tokenHelper;

        public MasterAuthManager(MasterDbContext context, ITokenHelper tokenHelper)
        {
            _context = context;
            _tokenHelper = tokenHelper;
        }

        public IDataResult<User> Login(UserForLoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == dto.Email);

            if (user == null)
                return new ErrorDataResult<User>("Kullanıcı Bulunamadı");

            if (!HashingHelper.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                return new ErrorDataResult<User>("Hatalı Parola");

            return new SuccessDataResult<User>(user);
        }

        public IResult UserExists(string email)
        {
            return _context.Users.Any(x => x.Email == email)
                ? new ErrorResult("Kullanıcı var")
                : new SuccessResult();
        }

        public IDataResult<AccessToken> CreateAccessToken(User user)
        {
            var claims = GetClaims(user);
            var accessToken = _tokenHelper.CreateToken(user, claims);
            return new SuccessDataResult<AccessToken>(accessToken, "Token Oluşturuldu");
        }

        private List<OperationClaim> GetClaims(User user)
        {
            var result = from o in _context.OperationClaims
                         join uo in _context.UserOperationClaims
                         on o.Id equals uo.OperationClaimId
                         where uo.UserId == user.Id
                         select new OperationClaim
                         {
                             Id = o.Id,
                             Name = o.Name
                         };

return result?.ToList() ?? new List<OperationClaim>();       
        }

        public IDataResult<User> Register(UserForRegisterDto dto, string password)
        {

            byte[] hash, salt;
            HashingHelper.CreateHash(password, out hash, out salt);

            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = hash,
                PasswordSalt = salt,
                Status = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return new SuccessDataResult<User>(user);
        }

    }
}
