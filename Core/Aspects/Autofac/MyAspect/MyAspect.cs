using Castle.DynamicProxy;
using Core.CrossCuttingConcern.Validataion;
using Core.Utilities.İnterceptors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Aspects.Autofac.MyIntereceptor
{
  public  class MyAspect:MethodInterception
    {
        protected override void OnBefore(IInvocation invocation)
        {//Method intercaptiondan kalıttım OnBefore

            Console.WriteLine($"[OnBefore] Metot başlıyor: {invocation.Method.Name}");

        }

        protected override void OnAfter(IInvocation invocation) 
        {

            Console.WriteLine($"[OnBefore] Metot başlıyor: {invocation.Method.Name}");

        }


    }

    }

