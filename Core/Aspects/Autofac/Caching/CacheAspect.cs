using Castle.DynamicProxy;
using Core.CrossCuttingConcern.Caching;
using Core.Utilities.IoC;
using Core.Utilities.İnterceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Core.Aspects.Autofac.Caching
{
    public class CacheAspect : MethodInterception
    {
        private int _duration;
        private ICacheManager _cacheManager;
        private IHttpContextAccessor _httpContextAccessor;

        public CacheAspect(int duration = 5)
        {
            _duration = duration;
            _cacheManager = ServiceTool.ServiceProvider.GetService<ICacheManager>();
            _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>();
        }

        public override void Intercept(IInvocation invocation)
        {
            var methodName = $"{invocation.Method.ReflectedType.FullName}.{invocation.Method.Name}";

            // 🔥 TENANT EKLE
            var tenant = _httpContextAccessor?.HttpContext?.Items["DatabaseName"]?.ToString() ?? "default";

            // 🔥 ARGÜMANLAR
            var arguments = invocation.Arguments.Select(x =>
            {
                if (x == null) return "<Null>";

                var value = x.ToString().Trim();

                if (string.IsNullOrEmpty(value))
                    return "<Empty>";

                return value.ToLower(new CultureInfo("tr-TR"));
            }).ToList();

            // 🔥 KEY (TENANT + METHOD + PARAMS)
            var key = $"{tenant}_{methodName}";

            if (arguments.Any())
            {
                key += $"({string.Join(",", arguments)})";
            }

            // 🔍 DEBUG
            Console.WriteLine($"CACHE KEY: [{key}]");

            // 🔥 CACHE CHECK
            if (_cacheManager.IsAdd(key))
            {
                Console.WriteLine($"CACHE HIT: [{key}]");
                invocation.ReturnValue = _cacheManager.Get(key);
                return;
            }

            invocation.Proceed();

            _cacheManager.Add(key, invocation.ReturnValue, _duration);

            Console.WriteLine($"CACHE ADDED: [{key}]");
        }
    }
}