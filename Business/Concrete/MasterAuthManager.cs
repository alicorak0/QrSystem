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
    public class MasterAuthManager : IMasterAuthService
    {
        private readonly IMasterUserService _userService;
        private readonly ITokenHelper _tokenHelper;

        public MasterAuthManager(
            IMasterUserService userService,
            ITokenHelper tokenHelper)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
        }

        public IDataResult<User> Login(UserForLoginDto dto)
        {
            var user = _userService.GetByMail(dto.Email);

            if (user == null)
                return new ErrorDataResult<User>("Kullanıcı Bulunamadı");

            if (!HashingHelper.VerifyPassword(
                dto.Password,
                user.PasswordHash,
                user.PasswordSalt))
            {
                return new ErrorDataResult<User>("Hatalı Parola");
            }

            return new SuccessDataResult<User>(user);
        }

        public IResult UserExists(string email)
        {
            if (_userService.GetByMail(email) != null)
            {
                return new ErrorResult("Kullanıcı var");
            }

            return new SuccessResult();
        }

        public IDataResult<AccessToken> CreateAccessToken(User user)
        {
            var claims = _userService.GetClaims(user);

            var accessToken = _tokenHelper.CreateToken(user, claims);

            return new SuccessDataResult<AccessToken>(
                accessToken,
                "Token Oluşturuldu");
        }

        public IDataResult<User> Register(
            UserForRegisterDto dto,
            string password)
        {
            byte[] hash, salt;

            HashingHelper.CreateHash(
                password,
                out hash,
                out salt);

            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = hash,
                PasswordSalt = salt,
                Status = true
            };

            _userService.Add(user);

            return new SuccessDataResult<User>(user);
        }
    }
}
