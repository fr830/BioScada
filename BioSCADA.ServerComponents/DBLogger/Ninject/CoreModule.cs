using System;
using BioSCADA.ServerComponents.DBLogger.Persistence.BusinessServices;
using BioSCADA.ServerComponents.DBLogger.Persistence.Repositories;
using NHibernate;
using Ninject.Activation;
using Ninject.Modules;

namespace GestContrat.Core.Ninject
{
    public delegate ISessionFactory OnSessionFactoryRequestDelegate();

    public class CoreModule : NinjectModule
    {

        public static event OnSessionFactoryRequestDelegate OnSessionFactoryRequest;


        public override void Load()
        {

            Bind<IUserRepository>().To<UserRepository>();
            Bind<IUserService>().To<UserService>();
            
            Bind<ISessionFactory>().ToMethod(SessionFactoryRequest);

            //Bind<I>().To<>();
            //Bind<I>().To<>();
        }

        private ISessionFactory SessionFactoryRequest(IContext arg)
        {
            if (OnSessionFactoryRequest != null)
                return OnSessionFactoryRequest();
            else
                throw new Exception("Session factory not found");
        }
    }
}
