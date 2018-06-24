using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Variables;
using BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers;
using BioSCADA.UnitTests.BioSCADA.ServerComponents.Timers;
using Moq;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents
{
    public class when_testing_server : Specification
    {
    }

    public class and_testing_server_in_action : when_testing_server
    {
        private const string expName = "Exp1";
        private Server server;
        private VariableStorageStub storage;
        private TimerStub timer;

        [SetUp]
        public void Setup()
        {

            server = new Server { Timer = new TimerStub() };
            IDriver driver = new DriverStub();
            Experiment exp = new Experiment() { Name = expName };
            server.AddExperiment(exp);
            exp.AddVariable(new IntegerVariable(1, "Var1") { Driver = driver });
            exp.AddVariable(new IntegerVariable(2, "Var2") { Driver = driver });
            exp.AddVariable(new DoubleVariable(3, "Var3") { Driver = driver, MinDeltaToReportChange = 0.5D });

            timer = new TimerStub();
            server.Timer = timer;

            storage = new VariableStorageStub();
            server.ValueStorage = storage;

            storage.Enqueue(1, 2.45);
            storage.Enqueue(2, 3.14);
            storage.Enqueue(3, 2.72);
        }

        [Test]
        public void then_server_notify_variable_changes()
        {
            bool read = false;
            server.OnVariablesChange += delegate(Dictionary<int, double> variableValues)
            {
                variableValues[1].ShouldEqual(2.45);
                variableValues[2].ShouldEqual(3.14);
                variableValues[3].ShouldEqual(2.72);
                read = true;
            };
            server.Start(0);
            timer.DoTick();
            Thread.Sleep(50);
            read.ShouldBeTrue();
        }

        [Test]
        public void then_server_notify_asynchroneously()
        {
            bool read = false;
            server.OnVariablesChange += delegate(Dictionary<int, double> variableValues)
            {
                variableValues[1].ShouldEqual(2.45);
                variableValues[2].ShouldEqual(3.14);
                variableValues[3].ShouldEqual(2.72);
                read = true;
            };
            server.Start(0);
            timer.DoTick();
            read.ShouldBeFalse();
        }

        [Test]
        public void then_server_sent_a_copy_of_internarl_variables()
        {
            bool goodValue = false;
            server.OnVariablesChange += delegate(Dictionary<int, double> variableValues)
            {
                variableValues[1] = 3.1416;
            };
            server.Start(0);
            timer.DoTick();
            Thread.Sleep(50);
            server.RequestVariables(expName,
                        (Dictionary<int, double> variableValues) =>
                        {
                            goodValue = variableValues[1] == 2.45;
                        });
            timer.DoTick();
            Thread.Sleep(50);
            goodValue.ShouldBeTrue();
        }

        [Test]
        public void then_server_ignore_notification_if_no_variable_changes()
        {
            bool read = false;
            server.OnVariablesChange += delegate(Dictionary<int, double> variableValues)
            {
                read = true;
            };
            server.Start(0);
            timer.DoTick();
            Thread.Sleep(50);
            read.ShouldBeTrue();

            storage.Enqueue(1, 2.45);
            storage.Enqueue(2, 3.14);
            storage.Enqueue(3, 2.72);
            read = false;
            timer.DoTick();
            Thread.Sleep(50);
            read.ShouldBeFalse();
        }

        [Test]
        public void then_server_notification_if_variable_double_changes_min_delta()
        {
            bool read = false;
            double changeDouble = 0;
            server.OnVariablesChange += delegate(Dictionary<int, double> variableValues)
            {
                if (variableValues.ContainsKey(3))
                    changeDouble = variableValues[3];
                read = true;
            };
            
            server.Start(0);
            timer.DoTick();
            Thread.Sleep(50);
            changeDouble.ShouldEqual(2.72);
            read.ShouldBeTrue();

            storage.Enqueue(1, 2.45);
            storage.Enqueue(2, 3.14);
            storage.Enqueue(3, 54.42);
            read = false;
            timer.DoTick();
            Thread.Sleep(50);
            changeDouble.ShouldEqual(54.42);
            read.ShouldBeTrue();
        }

        [Test]
        public void then_server_ignore_notification_if_variable_double_changes_min_delta()
        {
            bool read = false;
            double changeDouble = 0;
            server.OnVariablesChange += delegate(Dictionary<int, double> variableValues)
            {
                if (variableValues.ContainsKey(3))
                    changeDouble = variableValues[3];
                read = true;
            };

            server.Start(0);
            timer.DoTick();
            Thread.Sleep(50);
            changeDouble.ShouldEqual(2.72);
            read.ShouldBeTrue();

            storage.Enqueue(1, 2.45);
            storage.Enqueue(2, 3.14);
            storage.Enqueue(3, 2.42);
            read = false;
            timer.DoTick();
            Thread.Sleep(50);
            changeDouble.ShouldEqual(2.72);
            read.ShouldBeFalse();
        }

        [Test]
        public void then_server_notify_two_listeners()
        {
            bool read = false;
            bool read1 = false;

            server.OnVariablesChange += delegate(Dictionary<int, double> variableValues)
            {
                read = true;
            };

            server.OnVariablesChange += delegate(Dictionary<int, double> variableValues)
            {
                read1 = true;
            };
            server.Start(0);
            timer.DoTick();
            Thread.Sleep(50);
            read.ShouldBeTrue();
            read1.ShouldBeTrue();

        }

        [Test]
        public void then_can_update_variable_values_from_VariableStorage()
        {
            server.Start(0);
            timer.DoTick();
        }

        [Test]
        public void then_can_asynchroniously_request_server_variables()
        {
            server.Start(0);
            timer.DoTick();
            bool read = false;

            server.RequestVariables(expName,
                                    (Dictionary<int, double> variableValues) =>
                                    {
                                        variableValues[1].ShouldEqual(2.45);
                                        variableValues[2].ShouldEqual(3.14);
                                        variableValues[3].ShouldEqual(2.72);
                                        read = true;
                                    });

            timer.DoTick();
            Thread.Sleep(50);
            read.ShouldBeTrue();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Server has no assigment VariableStorage")]
        public void then_fail_if_variable_storage_unknown()
        {
            server.ValueStorage = null;

            server.Start(0);
            timer.DoTick();
        }

        [Test]
        public void then_uninitialized_values_are_NaN()
        {
            storage.Dequeue();

            server.Start(0);
            timer.DoTick();
            server.RequestVariables(expName, (Dictionary<int, double> variableValues) =>
            {
                variableValues[1].ShouldBeNaN();
                variableValues[2].ShouldEqual(3.14);
                variableValues[3].ShouldEqual(2.72);
            });
            timer.DoTick();
        }

    }

    public class and_testing_server_security : when_testing_server
    {
        private const string expName = "Exp1";
        private Server server;
        private VariableStorageStub storage;
        private TimerStub timer;

        [SetUp]
        public void Setup()
        {
            timer = new TimerStub();
            server = new Server
                         {
                             Timer = timer,
                             MinUserLevelToAdmin = 20,
                         };
            IDriver driver = new DriverStub();
            Experiment exp = new Experiment() { Name = expName };
            server.AddExperiment(exp, 100);
            exp.AddVariable(new IntegerVariable(1, "Var1")
                                {
                                    MinUserLevelToRead = 20,
                                    MinUserLevelToWrite = 40,
                                    Driver = driver
                                });
            exp.AddVariable(new IntegerVariable(2, "Var2") { Driver = driver });
            exp.AddVariable(new IntegerVariable(3, "Var3") { Driver = driver });

            storage = new VariableStorageStub();
            server.ValueStorage = storage;

            storage.Enqueue(1, 2.45);
            storage.Enqueue(2, 3.14);
            storage.Enqueue(3, 2.72);
        }

        [Test]
        public void then_accessing_variable_with_lower_permision_returns_negative_infinite()
        {
            server.Start(0);
            timer.DoTick();
            server.RequestVariables(expName,
                                    (Dictionary<int, double> variableValues) =>
                                    {
                                        variableValues[1].ShouldEqual(double.NegativeInfinity);
                                        variableValues[2].ShouldEqual(3.14);
                                        variableValues[3].ShouldEqual(2.72);
                                    });
            timer.DoTick();
        }

        [Test]
        public void then_changing_variable_with_lower_permission_returns_false()
        {
            server.Start(0);
            timer.DoTick();
            server.WriteVariable(expName, 1, 50.45).ShouldEqual(false);
        }

        [Test]
        public void then_changing_variable_with_lower_permission_returns_false_even_having_read_permission()
        {
            server.Start(0);
            timer.DoTick();
            server.WriteVariable(expName, 1, 50, 20).ShouldEqual(false);
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "Cannot add experiment with current user permission")]
        public void then_cannot_add_experiment_with_lower_permission()
        {
            server.AddExperiment(new Experiment { Name = "Exp3" });
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "Cannot remove experiment with current user permission")]
        public void then_cannot_remove_experiment_with_lower_permission()
        {
            server.RemoveExperiment(expName);
        }

    }

    public class base_tests : when_testing_server
    {
        private const string expName = "Exp1";
        private Server server;
        private VariableStorageStub storage;

        [SetUp]
        public void Setup()
        {
            server = new Server { Timer = new TimerStub() };
            IDriver driver = new DriverStub();
            Experiment exp = new Experiment() { Name = expName };
            server.AddExperiment(exp);
            exp.AddVariable(new IntegerVariable(1, "Var1") { Driver = driver });
            exp.AddVariable(new IntegerVariable(2, "Var2") { Driver = driver });
            exp.AddVariable(new IntegerVariable(3, "Var3") { Driver = driver });

            storage = new VariableStorageStub();
            server.ValueStorage = storage;

            storage.Enqueue(1, 2.45);
            storage.Enqueue(2, 3.14);
            storage.Enqueue(3, 2.72);
        }




        [Test]
        public void then_requesting_variables_on_server_stopped_returns_false()
        {
            server.Start(0);
            server.Stop(0);
            bool requested = server.RequestVariables(expName,
                                    (Dictionary<int, double> variableValues) =>
                                    {
                                    });
            requested.ShouldBeFalse();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Unknown experiment: Cuco")]
        public void then_fail_asynch_request_on_unknown_experiment()
        {
            server.Start(0);
            server.RequestVariables("Cuco", (Dictionary<int, double> variableValues) =>
                                                {
                                                    variableValues[1].ShouldEqual(2.45);
                                                    variableValues[2].ShouldEqual(3.14);
                                                    variableValues[3].ShouldEqual(2.72);
                                                });
        }



        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Cannot change VariableStorer on running server")]
        public void then_fail_if_change_variable_storage_on_server_running()
        {
            server.Start(0);
            server.ValueStorage = null;
        }
    }

    public class and_testing_start_stop_server : when_testing_server
    {
        private Server server;
        private TimerStub timerStub;

        [SetUp]
        public void Setup()
        {
            server = new Server
                         {
                             MinUserLevelToStart = 20,
                         };
            server.AddExperiment(new Experiment { Name = "Exp1" });
            VariableStorageStub storage = new VariableStorageStub();
            server.ValueStorage = storage;

            timerStub = new TimerStub();
            server.Timer = timerStub;
        }

        [Test]
        public void then_can_start_server()
        {
            bool executed = false;
            server.Start(20);
            server.RequestVariables("Exp1",
                                    (Dictionary<int, double> variableValues) =>
                                    {
                                        executed = true;
                                    });

            timerStub.DoTick();
            Thread.Sleep(50);
            executed.ShouldBeTrue();
        }

        [Test]
        public void when_server_stops_all_drivers_stop()
        {
            server.AddDriver(new DriverStub() { Started = true });
            server.AddDriver(new DriverStub() { Started = false });
            server.AddDriver(new DriverStub() { Started = true });
            server.Start(100);
            server.Stop(100);
            server.GetDrivers().All(x => !x.Started).ShouldBeTrue();
        }

        [Test]
        public void when_server_starts_all_drivers_starts()
        {
            server.AddDriver(new DriverStub() { Started = true });
            server.AddDriver(new DriverStub() { Started = false });
            server.AddDriver(new DriverStub() { Started = true });
            server.Start(100);
            server.GetDrivers().All(x => x.Started).ShouldBeTrue();
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "User does not have permissions to Start server")]
        public void then_fail_starting_server_without_permission()
        {
            server.Start(0);
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "User does not have permissions to Stop server")]
        public void then_fail_stopping_server_without_permission()
        {
            server.Start(20);
            server.Stop(-10);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot change timer on running server")]
        public void then_fail_changing_timer_on_running_server()
        {
            server.RequestVariables("Exp1",
                                    (Dictionary<int, double> variableValues) =>
                                    {
                                    });

            server.Start(20);
            server.Timer = new TimerStub();
        }

        [Test]
        public void then_can_stop_server()
        {
            bool executed = false;
            server.Start(20);
            server.RequestVariables("Exp1",
                                    (Dictionary<int, double> variableValues) =>
                                    {
                                        executed = true;
                                    });

            timerStub.DoTick();
            Thread.Sleep(50);
            executed.ShouldBeTrue();
            executed = false;

            server.Stop(20);
            timerStub.DoTick();
            Thread.Sleep(50);
            executed.ShouldBeFalse();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Cannot start server with timer not assigned")]
        public void then_fail_starting_server_with_timer_not_assigned()
        {
            server.Timer = null;

            bool executed = false;
            server.RequestVariables("Exp1",
                                    (Dictionary<int, double> variableValues) =>
                                    {
                                        executed = true;
                                    });

            server.Start(20);
            timerStub.DoTick();
            executed.ShouldBeTrue();
        }

    }

    public class and_testing_handling_drivers : when_testing_server
    {
        private Server server;

        [SetUp]
        public void Setup()
        {
            server = new Server();
            server.ValueStorage = new VariableStorageStub();
            server.Timer = new TimerStub();
        }

        [Test]
        public void then_can_add_driver()
        {
            server.AddDriver(new DriverStub());
        }

        [Test]
        public void then_can_enumerate_drivers()
        {
            server.AddDriver(new DriverStub());
            IEnumerable<IDriver> drivers = server.GetDrivers();
            drivers.Count().ShouldEqual(1);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot add driver while server running")]
        public void then_fail_adding_driver_while_server_running()
        {
            server.Start(0);
            server.AddDriver(new DriverStub());
        }

        [Test]
        public void then_can_remove_driver()
        {
            IDriver driverStub = new DriverStub();
            server.AddDriver(driverStub);
            server.RemoveDriver(driverStub);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Cannot remove driver from running server")]
        public void then_fail_removing_driver_with_server_started()
        {
            IDriver driverStub = new DriverStub();
            server.AddDriver(driverStub);
            server.Start(0);
            server.RemoveDriver(driverStub);
        }
    }

    public class and_testing_handling_experiments : when_testing_server
    {
        private const string experimentName = "Exp1";
        private Experiment experiment;
        private Server server;

        [SetUp]
        public void Setup()
        {
            server = new Server();
            server.Timer = new TimerStub();
            server.ValueStorage = new VariableStorageStub();
            experiment = new Experiment() { Name = experimentName };
            server.AddExperiment(experiment);
        }

        [Test]
        public void then_can_add_experiment()
        {
            server.GetExperiments().Count().ShouldEqual(1);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot add experiment with empty Name")]
        public void then_fail_adding_experiment_with_empty_Name()
        {
            server.AddExperiment(new Experiment());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot add experiment with dupplicate name: Exp1")]
        public void then_fail_adding_experiment_with_dupplicate_Name()
        {
            server.AddExperiment(new Experiment() { Name = "Exp1" });
        }

        [Test]
        public void then_can_remove_experiment()
        {
            server.RemoveExperiment(experimentName);
            server.GetExperiments().ShouldBeEmpty();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot add experiment to running server")]
        public void then_fail_if_add_experiment_with_server_started()
        {
            server.Start(0);
            server.AddExperiment(new Experiment() { Name = "Exp1" });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot remove experiment to running server")]
        public void then_fail_removing_experiment_if_server_started()
        {
            server.Start(0);
            server.RemoveExperiment(experimentName);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot remove unexisting experiment")]
        public void then_fail_removing_unexisting_experiment()
        {
            server.RemoveExperiment("Exp2");
        }

        [Test]
        public void then_can_access_experiment_by_name()
        {
            server[experimentName].ShouldEqual(experiment);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot access unexistent experiment [Exp2]")]
        public void then_fail_if_access_unknown_experiment()
        {
            var x = server["Exp2"];
        }
    }

    public class and_testing_write_variable : when_testing_server
    {
        private const string expName = "Exp1";
        private Server server;
        private int varId = 12;
        private double newValue = 3.14;

        [SetUp]
        public void Setup()
        {
            server = new Server();
            Experiment exp = new Experiment() { Name = expName };
            server.AddExperiment(exp);
            Mock<IDriver> driver = new Mock<IDriver>(MockBehavior.Strict);
            driver
                .Setup(x => x.WriteValue(varId, newValue));

            Variable variable = new IntegerVariable(varId, "Var1") { Driver = driver.Object };
            exp.AddVariable(variable);
        }

        [Test]
        public void then_can_write_variable_value()
        {
            server.WriteVariable(expName, varId, newValue);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Unknown experiment: Exp2")]
        public void then_fail_if_unknown_experiment()
        {
            server.WriteVariable("Exp2", varId, newValue);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Unknown variable: ID=14")]
        public void then_fail_if_unknown_variable()
        {
            server.WriteVariable("Exp1", 14, newValue);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Variable [12] has no driver")]
        public void then_fail_if_variable_has_no_driver()
        {
            server[expName][varId].Driver = null;

            server.WriteVariable("Exp1", 12, newValue);
        }

    }
}
