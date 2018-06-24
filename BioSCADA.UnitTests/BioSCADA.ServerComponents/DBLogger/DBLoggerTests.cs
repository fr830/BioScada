using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Alarms;
using BioSCADA.ServerComponents.DBLogger.Persistence.BusinessServices;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;
using BioSCADA.ServerComponents.Drivers.DriversTest;
using BioSCADA.ServerComponents.Variables;
using BioSCADA.UnitTests.BioSCADA.ServerComponents.Alarms;
using BioSCADA.UnitTests.BioSCADA.ServerComponents.Timers;
using NUnit.Framework;
using BioSCADA.ServerComponents.DBLogger;
using NBehave.Spec.NUnit;
using Moq;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger
{
    public class when_testing_DBLoggerTests : Specification
    {
        private VariableLogger logger;
        
        [Test]
        public void Cuando_recive_valores_son_salvados_solo_los_que_tienen_que_ser_salvados()
        {
            Experiment exp = new Experiment();
            IDriver driver = new DriverInt(){ValueStorage = new ValueStorage()};

            IntegerVariable integerVariable = new IntegerVariable(0, "var1") { Driver = driver, MinValue = 0, MaxValue = 16, LogValues = true };
            exp.AddVariable(integerVariable); // 5 bits
            IntegerVariable integerVariable1 = new IntegerVariable(1, "var2") { Driver = driver, MinValue = 0, MaxValue = 1015 };
            exp.AddVariable(integerVariable1); // 10 bits
            BoolVariable boolVariable = new BoolVariable(2, "var3") { Driver = driver, LogValues = true };
            exp.AddVariable(boolVariable); // 1 bits
            driver.AddVariable(integerVariable);
            driver.AddVariable(integerVariable1);
            driver.AddVariable(boolVariable);
            logger = new VariableLogger(new List<Experiment>() { exp });
            Dictionary<int, double> varValues = new Dictionary<int, double>
                                                    {
                                                        {0, 23},
                                                        {1, 12},
                                                    };
            logger.ExperimentsLogger.Count.ShouldEqual(1);
            ExperimentLoggerStub stub = new ExperimentLoggerStub(exp);
            logger.ExperimentsLogger.RemoveAt(0);
            logger.ExperimentsLogger.Add(stub);
            logger.ReceiveVariableValues(varValues);
            stub.ValuesReceive[0].ShouldEqual(23);
            stub.ValuesReceive[1].ShouldEqual(12);
        }

        [Test]
        public void when_zip_files_on_day()
        {
            Experiment exp = new Experiment();
            exp.Name = "Exp1";
            IDriver driver = new DriverInt() { ValueStorage = new ValueStorage() };

            IntegerVariable integerVariable = new IntegerVariable(0, "var1") { Driver = driver, MinValue = 0, MaxValue = 16, LogValues = true };
            exp.AddVariable(integerVariable); // 5 bits
            IntegerVariable integerVariable1 = new IntegerVariable(1, "var2") { Driver = driver, MinValue = 0, MaxValue = 1015 };
            exp.AddVariable(integerVariable1); // 10 bits
            BoolVariable boolVariable = new BoolVariable(2, "var3") { Driver = driver, LogValues = true };
            exp.AddVariable(boolVariable); // 1 bits
            driver.AddVariable(integerVariable);
            driver.AddVariable(integerVariable1);
            driver.AddVariable(boolVariable);
            logger = new VariableLogger(new List<Experiment>() { exp });
            exp.Start(20);
            Mock<IDateTimeProvider> mock = new Mock<IDateTimeProvider>(MockBehavior.Strict);
            logger.TimeProvider = mock.Object;
            mock.Setup(x => x.Now).Returns(DateTime.Now);
            Dictionary<int, double> varValues = new Dictionary<int, double>
                                                    {
                                                        {0, 23},
                                                        {1, 12},
                                                    };
            logger.ReceiveVariableValues(varValues);
            mock.Setup(x => x.Now).Returns(new DateTime(2012,5,12));
            logger.ReceiveVariableValues(varValues);
        }

        
    }

}
