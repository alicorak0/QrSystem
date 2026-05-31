using Castle.DynamicProxy;
using Core.CrossCuttingConcern.Validataion;
using Core.Utilities.İnterceptors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.CrossCuttingConcern.Validataion;
namespace Core.Aspects.Autofac.Validation
{
   public class ValidationAspect:MethodInterception
    {  //Bu bir attribute valdate attributesi
        private Type _validatorType;
        public ValidationAspect(Type validatorType)
        {
            if (!typeof(IValidator).IsAssignableFrom(validatorType))
            {
                throw new System.Exception("Bu Bir doğrulama sınıfı değil");
            }

            _validatorType = validatorType;
         }


        //Method ıntercepiondan ezdiği bir metot
        protected override void OnBefore(IInvocation invocation)
        {//Method intercaptiondan kalıttım OnBefore

            var validator = (IValidator)Activator.CreateInstance(_validatorType);
            var entityType = _validatorType.BaseType.GetGenericArguments()[0];
            var entities = invocation.Arguments.Where(t => t.GetType() == entityType); //entity seçiliyor paramtreler ile bsae classın   generic ile
            foreach (var entity in entities)
            {
                ValidataionTool.Validate(validator, entity); //Normalde Product manager içinde olurdu
            }
        }
    }
}
