using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.DBLogger;
using BioSCADA.ServerComponents.DBLogger.Persistence;
using BioSCADA.ServerComponents.DBLogger.Persistence.BusinessServices;
using BioSCADA.ServerComponents.DBLogger.Persistence.Repositories;
using FluentNHibernate;
using NHibernate;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger
{
    public class when_testing_user : Specification
    {
        [Test]
        public void CreateDatabaseInMemory()
        {
            string connectionString = "Data Source=:memory:;Version=3;New=True;";
            TestConnection(connectionString);
        }

        [Test]
        public void CreateDatabaseInDisk()
        {
            string connectionString = @"Data Source=BioSCADA.sqlite;Version=3;New=True;";
            TestConnection(connectionString);
        }

        private void TestConnection(string connectionString)
        {

            SessionSource sessionSource = SqLiteConnector.CreateDebugSessionSource(connectionString);

            using (ISession session = sessionSource.CreateSession())
            {
                sessionSource.BuildSchema(session);
            }
        }

        [Test]
        public void Insert_User_In_DB()
        {
            UserService userService = new UserService(new UserRepository(DbHandlerSingleton.GetInstance().SessionSource.CreateSession()));
            userService.SaveOrUpdate(new User() {Name = "guille", Password = "guille", UserLever = 20});
            userService.GetAll().Count.ShouldEqual(1);
        }

    }
}
