using Core.Utilities.IoC;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.IoC;

namespace Core.Extensions
{
   public static class ServiceCollectionExtensions
    {        
        //Genişletilecek metot
        public static IServiceCollection AddDependencyResolvers(this IServiceCollection serviceCollection, ICoreModule[] modules)
        {
            foreach (var module in modules) //her bir modül için yükle
            {
                module.Load(serviceCollection);
                
            }

            return ServiceTool.Create(serviceCollection);
        }


    }
}
