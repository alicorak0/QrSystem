using Castle.DynamicProxy;

namespace Core.Utilities.İnterceptors
{
    public partial class Class1
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public abstract class MethodInterceptionBaseAttribute : Attribute, Castle.DynamicProxy.IInterceptor
        {
            public int Priority { get; set; } // hangi ATTRİBUT ÖNCE ÇALIŞSIN

            public virtual void Intercept(IInvocation invocation)
            {

            }
        }
    }


}
