using FluentNHibernate;
using NHibernate.Context;

namespace BioSCADA.ServerComponents.DBLogger.Persistence
{
    public class DbHandlerSingleton
    {
        private static DbHandlerSingleton instance = new DbHandlerSingleton();
        public static DbHandlerSingleton GetInstance()
        {
            return instance;
        }

        private ISessionSource sessionSource;

        public ISessionSource SessionSource
        {
            get
            {
                if (sessionSource == null)
                {
                    string connectionString = @"Data Source=BioSCADA.sqlite;Version=3;New=True;";
                    sessionSource = SqLiteConnector.CreateSessionSource(connectionString);
                    CurrentSessionContext.Bind(sessionSource.CreateSession());
                }
                return sessionSource;
            }
        }

    }
}
