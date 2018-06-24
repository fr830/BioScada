using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Alarms;
using BioSCADA.ServerComponents.DBLogger;
using BioSCADA.ServerComponents.DBLogger.Persistence;
using BioSCADA.ServerComponents.DBLogger.Persistence.BusinessServices;
using BioSCADA.ServerComponents.DBLogger.Persistence.Repositories;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;
using BioSCADA.ServerComponents.Drivers;
using BioSCADA.ServerComponents.Drivers.DriversTest;
using BioSCADA.ServerComponents.Timers;
using BioSCADA.ServerComponents.Variables;
using BioSCADA.WCFServices;
using FluentNHibernate;
using NHibernate;
using NHibernate.Context;

namespace BioSCADA.ServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var service = InitializeDatabase();

            Server server = new Server
                                {
                                    Timer = new RealtimeFixedIntervalTimer { Interval = 250 }
                                };


            Uri uri1 = new Uri(ConfigurationManager.AppSettings["addr"]);
            ServiceHost host = new ServiceHost(typeof(ServerService), uri1);
            host.Open();
            Console.WriteLine("SCADA SecuritySvc listen {0}", uri1.ToString());

            ValueStorage storage = new ValueStorage();
            server.ValueStorage = storage;

            AlarmManager manager = new AlarmManager { TimeProvider = new DateTimeProvider() };
            manager.Alarms.Add(new Alarm
                                        {
                                            AlarmID = 1,
                                            Sensor = new RangeAlarmSensor() { MaxBound = 190, MinBound = 160 },
                                            VariableID = 1
                                        });


            server.OnVariablesChange += manager.ReceiveVariableValues;

            /*IDriver driver = new DriverModbusRTU
            {
                ValueStorage = storage,
                Timer = new RealtimeFixedIntervalTimer { Interval = 250 }
            };
            ModbusStationRTU stationRtu = new ModbusStationRTU("Station1", 1,
                new SerialComm("COM1", 9600, 8, Parity.None, StopBits.One), true);

            Variable var1, var2, var3, var4, var5, var6, var7, var8;
            driver.AddVariable(var1 = new Variable(1, "Out40") { TicksToSample = 2, Driver = driver, DriverConfiguration = new ModbusConfiguration(stationRtu, 64, ModbusTypeData.Coils), VariableSerialize = new BoolVariableSerializer() { PositionAllData = 0, PositionInByte = 0 } });
            driver.AddVariable(var2 = new Variable(2, "In00") { TicksToSample = 2, Driver = driver, DriverConfiguration = new ModbusConfiguration(stationRtu, 0, ModbusTypeData.Coils) });
            driver.AddVariable(var3 = new Variable(3, "In02") { TicksToSample = 2, Driver = driver, DriverConfiguration = new ModbusConfiguration(stationRtu, 2, ModbusTypeData.Coils) });
            driver.AddVariable(var4 = new Variable(4, "In04") { TicksToSample = 2, Driver = driver, DriverConfiguration = new ModbusConfiguration(stationRtu, 4, ModbusTypeData.Coils) });
            driver.AddVariable(var5 = new Variable(5, "Timer1") { TicksToSample = 2, Driver = driver, DriverConfiguration = new ModbusConfiguration(stationRtu, 20483, ModbusTypeData.Inputs_Registers) });
            driver.AddVariable(var6 = new Variable(6, "Timer2") { TicksToSample = 2, Driver = driver, DriverConfiguration = new ModbusConfiguration(stationRtu, 20484, ModbusTypeData.Inputs_Registers) });
            driver.AddVariable(var7 = new Variable(7, "Timer3") { TicksToSample = 2, Driver = driver, DriverConfiguration = new ModbusConfiguration(stationRtu, 20485, ModbusTypeData.Inputs_Registers) });
            driver.AddVariable(var8 = new Variable(8, "Out46") { TicksToSample = 2, Driver = driver, DriverConfiguration = new ModbusConfiguration(stationRtu, 70, ModbusTypeData.Coils) });
            */IDriver driver = new DriverInt
            {
                ValueStorage = storage,
                Timer = new RealtimeFixedIntervalTimer { Interval = 250 }
            };
            Variable var1, var2, var3, var4, var5, var6, var7;
            driver.AddVariable(var1 = new IntegerVariable(1, "Out40") { TicksToSample = 2, Driver = driver, });
            driver.AddVariable(var2 = new IntegerVariable(2, "In00") { TicksToSample = 1, Driver = driver, LogValues = true, MinValue = 0, MaxValue = 230});
            driver.AddVariable(var3 = new IntegerVariable(3, "In02") { TicksToSample = 1, Driver = driver, LogValues = true, MinValue = 0, MaxValue = 230});
            driver.AddVariable(var4 = new IntegerVariable(4, "In04") { TicksToSample = 1, Driver = driver, LogValues = true, MinValue = 0, MaxValue = 230});
            driver.AddVariable(var5 = new IntegerVariable(5, "Timer1") { TicksToSample = 2, Driver = driver });
            driver.AddVariable(var6 = new IntegerVariable(6, "Timer2") { TicksToSample = 2, Driver = driver });
            driver.AddVariable(var7 = new IntegerVariable(7, "Timer3") { TicksToSample = 2, Driver = driver });

            server.AddDriver(driver);

            Experiment experiment = new Experiment { Name = "Exp1" };
            experiment.AddVariable(var1);
            experiment.AddVariable(var2);
            experiment.AddVariable(var3);
            experiment.AddVariable(var4);
            experiment.AddVariable(var5);
            experiment.AddVariable(var6);
            experiment.AddVariable(var7);
            //experiment.AddVariable(var8);

            server.AddExperiment(experiment);
            //VariableLogger logger = new VariableLogger(new List<Experiment>() { experiment });
            //server.OnVariablesChange += logger.ReceiveVariableValues;
            


            server.Start(0);
            experiment.Start(0);

            ThreadPool.QueueUserWorkItem(myThead, server);


            Console.Read();
            server.Stop(0);
            Console.Read();
            host.Abort();
            host.Close();
        }

        //private static ILoggerDataService InitializeDatabase()
        //{
        //    Console.WriteLine("Initializing database...");
        //    string connectionString = @"Data Source=Historic.sqlite;Version=3;New=True;";
        //    SessionSource sessionSource = SqLiteConnector.CreateSessionSource(connectionString);
        //    sessionSource.BuildSchema();
        //    ISession session = sessionSource.CreateSession();
        //    ILoggerDataService service = new LoggerDataService(new LoggerDataRepository(session));
        //    return service;
        //}

        private static void myThead(object o)
        {
            var server = (o as Server);
            while (true)
            {
                Thread.Sleep(1000);
                server.RequestVariables("Exp1", (values) =>
                {
                    foreach (var value in values)
                    {
                        Console.WriteLine(value.Key + " " + value.Value);
                    }
                    Console.WriteLine("-----------------------");
                });

            }
        }
    }
}
