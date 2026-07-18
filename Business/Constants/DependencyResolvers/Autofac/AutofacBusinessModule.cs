using Autofac;
using Business.Concrete;
using DataAccess.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Abstract;
using DataAccess.Concrete.EntityFramework;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Core.Utilities.İnterceptors;
using Business.CCS;
using Core.Utilities.Security.JWT;
using Microsoft.AspNetCore.Http;


namespace Business.Constants.DependencyResolvers.Autofac
{
   public class AutofacBusinessModule:Module
    {

        protected override void Load(ContainerBuilder builder)
        {     //IProductService isterse ProductManager verilir
            builder.RegisterType<ProductManager>().As<IProductService>().SingleInstance(); // kim isterse aynı objeyi referansı verir
            builder.RegisterType<EfProductDal>().As<IProductDal>().SingleInstance(); // kim isterse aynı objeyi referansı verir
                                                                                     // builder.RegisterType<FileLogger>().As<ILogger>().SingleInstance(); // kim isterse aynı objeyi referansı verir
                                                                                     //Logeri kaydetme  birisi ILogger isterse arka planda oluştur FileLoger
             //for categories

            builder.RegisterType<CategoryManager>().As<ICategoryService>().SingleInstance(); // kim isterse aynı objeyi referansı verir
            builder.RegisterType<EfCategoryDal>().As<ICategoryDal>().SingleInstance(); // kim isterse aynı objeyi referansı verir

            builder.RegisterType<AllergenManager>().As<IAllergenService>().SingleInstance();
            builder.RegisterType<EfAllergenDal>().As<IAllergenDal>().SingleInstance();

            //authorization and login/sign in
            builder.RegisterType<UserManager>().As<IUserService>().SingleInstance(); // kim isterse aynı objeyi referansı verir
            builder.RegisterType<EfUserDal>().As<IUserDal>().SingleInstance(); // kim isterse aynı objeyi referansı verir

            builder.RegisterType<AuthManager>().As<IAuthService>().SingleInstance(); // kim isterse aynı objeyi referansı verir
            builder.RegisterType<JwtHelper>().As<ITokenHelper>().SingleInstance(); // kim isterse aynı objeyi referansı verir
            builder.RegisterType<MasterAuthManager>().As<IMasterAuthService>().SingleInstance(); // kim isterse aynı objeyi referansı verir
            builder.RegisterType<R2StorageService>().As<IR2StorageService>().SingleInstance();


            builder.RegisterType<MasterUserManager>().As<IMasterUserService>().SingleInstance(); // kim isterse aynı objeyi referansı verir
            builder.RegisterType<EfMasterUserDal>().As<IMasterUserDal>().SingleInstance(); // kim isterse aynı objeyi referansı verir


            //   builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance(); // kim isterse aynı objeyi referansı verir







            //validation kısmını etkinleltir startup aşamasında
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
                .EnableInterfaceInterceptors(new ProxyGenerationOptions()
                {
                    Selector = new AspectInterceptorSelector()
                }).SingleInstance();

        }
    }
}
