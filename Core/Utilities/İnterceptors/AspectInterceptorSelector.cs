using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Utilities.İnterceptors.Class1;
using System.Reflection;

namespace Core.Utilities.İnterceptors
{
 

    public class AspectInterceptorSelector : Castle.DynamicProxy.IInterceptorSelector 
    {
        public Castle.DynamicProxy.IInterceptor[] SelectInterceptors(Type type, MethodInfo method, Castle.DynamicProxy.IInterceptor[] interceptors)
        {
            var classAttributes = type.GetCustomAttributes<MethodInterceptionBaseAttribute>
                (true).ToList();
            var methodAttributes = type.GetMethod(method.Name)
                .GetCustomAttributes<MethodInterceptionBaseAttribute>(true);
            classAttributes.AddRange(methodAttributes);
            //classAttributes.Add(new ExceptionLogAspect(typeof(FileLogger))); //Otomatik olarak sitemdeki tüm metotları Log'a davet eder
            //Loglama alt yapısı hazırlayacaz

            return classAttributes.OrderBy(x => x.Priority).ToArray();
        }
    }


}
