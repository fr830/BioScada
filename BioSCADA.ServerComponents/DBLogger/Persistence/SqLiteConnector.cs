using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.ByteCode.LinFu;
using NHibernate.Dialect;

namespace BioSCADA.ServerComponents.DBLogger.Persistence
{
    public class SqLiteConnector
    {
        public static SessionSource CreateSessionSource(string connectionString)
        {
            var dbConfig = SQLiteConfiguration.Standard
                .ConnectionString(connectionString)
                .Dialect<SQLiteDialect>();

            var cfg = Fluently.Configure()
                .Database(dbConfig)
                .ProxyFactoryFactory(typeof(ProxyFactoryFactory))
                .ExposeConfiguration(
                    c => c.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass, "thread_static"))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<SqLiteConnector>());

            return new SessionSource(cfg);
        }
        
        public static SessionSource CreateDebugSessionSource(string connectionString)
        {
            var dbConfig = SQLiteConfiguration.Standard
                .ConnectionString(connectionString)
                .ShowSql()
                .Dialect<SQLiteDialect>();

            var cfg = Fluently.Configure()
                .Database(dbConfig)
                .ProxyFactoryFactory(typeof(ProxyFactoryFactory))
                .ExposeConfiguration(
                    c => c.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass, "thread_static"))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<SqLiteConnector>());

            return new SessionSource(cfg);
        }
    }

}
