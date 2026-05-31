using Business.Constants;
using Castle.DynamicProxy;
using Core.Utilities.İnterceptors;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.IoC;
using Microsoft.Extensions.DependencyInjection;
using Core.Extensions;
using Core.CrossCuttingConcern.Exceptions;

namespace Business.BusinessAspects.Autofac
{        //Secured OPERATİON İÇİN DOĞRULAMA İŞLEMLERİ

    public class SecuredOperation : MethodInterception
    {
        private string[] _roles;
        private IHttpContextAccessor _httpContextAccessor;

        public SecuredOperation(string roles)
        {
            _roles = roles.Split(','); //attribute çağırıken rolleri split edip array'e alıyorum
            _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>();
            //HTTP context’e erişim sağlar, yani HttpContext.User üzerinden kullanıcının claim’lerini okuyabiliriz
        }        //service Tool ile erişiriz httpcontext'e

        protected override void OnBefore(IInvocation invocation)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // 1 Context veya User yoksa → AuthenticationException (401)
            if (httpContext == null || httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
            {
                throw new AuthenticationException(Messages.AuthenticationError);
            }


            //Claims Kotrolleri
            var roleClaims = _httpContextAccessor.HttpContext.User.ClaimRoles(); //ClaimRoles() metodu token içindeki tüm rol claimlerini alıyor ve listeye çeviriyor.

            _httpContextAccessor.HttpContext.Response.Headers
                .Add("X-User-Roles", string.Join(",", roleClaims));
            foreach (var role in _roles)
            {
                if (roleClaims.Contains(role))  //Rolleri kontrol et 
                {
                    return;
                }
            }
            throw new AuthorizationDeniedException(Messages.AuthorizationDenied);
        }
    }
}
