using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Alarms;
using BioSCADA.ServerComponents.Drivers;
using BioSCADA.ServerComponents.Timers;
using BioSCADA.ServerComponents.Variables;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace BioSCADA.IntegrationTests.BioSCADA.ServerComponents.ServerIntegrated
{
    public class when_testing_server
    {
        [Test]
        public void then_work_with_one_driver_one_experiment()
        {
            Server server = new Server { Timer = new RealtimeFixedIntervalTimer { Interval = 20 } };
            ValueStorage storage = new ValueStorage();
            server.ValueStorage = storage;

            IDriver driver = new OneValueTestingDriver
                                 {
                                     ValueStorage = storage,
                                     Timer = new RealtimeFixedIntervalTimer { Interval = 20 },
                                     ValueToPostToVariableStorage = Tuple.Create(1, 3.14),
                                 };
            Variable var1, var2, var3;
            driver.AddVariable(var1 = new IntegerVariable(1, "Var1"));
            driver.AddVariable(var2 = new IntegerVariable(2, "Var2"));
            driver.AddVariable(var3 = new IntegerVariable(3, "Var3"));

            server.AddDriver(driver);

            Experiment experiment = new Experiment { Name = "Exp1" };
            experiment.AddVariable(var1);
            experiment.AddVariable(var2);
            experiment.AddVariable(var3);

            server.AddExperiment(experiment);

            server.Start(0);
            experiment.Start(0);

            Thread.Sleep(100);

            server.RequestVariables("Exp1", (values) =>
                                                {
                                                    values.Count.ShouldEqual(3);
                                                    values[1].ShouldEqual(3.14);
                                                    values[2].ShouldBeNaN();
                                                    values[3].ShouldBeNaN();
                                                    Debug.WriteLine("Done!");
                                                });

            Thread.Sleep(100);

        }

        [Test]
        public void then_work_heavy_scenario_single_instances()
        {
            Server server = new Server { Timer = new RealtimeFixedIntervalTimer { Interval = 20 } };
            ValueStorage storage = new ValueStorage();
            server.ValueStorage = storage;

            IDriver driver = new AllValuesTestingDriver
            {
                ValueStorage = storage,
                Timer = new RealtimeFixedIntervalTimer { Interval = 20 },
                ValueToPost = 3.14
            };
            Experiment experiment = new Experiment { Name = "Exp1" };

            int varCount = 1000;

            for (int i = 0; i < varCount; i++)
            {
                var variable = new IntegerVariable(i, "Var" + i);
                driver.AddVariable(variable);
                experiment.AddVariable(variable);
            }
            server.AddDriver(driver);
            server.AddExperiment(experiment);

            server.Start(0);
            experiment.Start(0);

            Thread.Sleep(50);

            server.RequestVariables("Exp1", (values) =>
            {
                values.Count.ShouldEqual(varCount);
                for (int i = 0; i < varCount; i++)
                    values[i].ShouldEqual(3.14);
                Debug.WriteLine("Done!");
            });

            Thread.Sleep(100);

        }

       
    }

    internal class OneValueTestingDriver : BaseDriver
    {
        public List<Tuple<int, double>> ValuesToWrite = new List<Tuple<int, double>>();
        public Tuple<int, double> ValueToPostToVariableStorage;

        protected override void DoProcess(Dictionary<int, bool> variablesToRead)
        {
            SendValueToStorage(ValueToPostToVariableStorage.Item1, ValueToPostToVariableStorage.Item2);
        }

        protected override void DoWriteValue(int variableId, double newValue)
        {
            ValuesToWrite.Add(Tuple.Create(variableId, newValue));
        }
    }

    internal class AllValuesTestingDriver : BaseDriver
    {
        public List<Tuple<int, double>> ValuesToWrite = new List<Tuple<int, double>>();
        public double ValueToPost { get; set; }

        protected override void DoProcess(Dictionary<int, bool> variablesToRead)
        {
            foreach (var varId in _variables.Keys)
                SendValueToStorage(varId, ValueToPost);
        }

        protected override void DoWriteValue(int variableId, double newValue)
        {
            ValuesToWrite.Add(Tuple.Create(variableId, newValue));
        }
    }

}
