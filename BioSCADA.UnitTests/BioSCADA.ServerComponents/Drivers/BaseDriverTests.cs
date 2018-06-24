using System;
using System.Collections;
using System.Collections.Generic;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Drivers;
using BioSCADA.ServerComponents.Variables;
using BioSCADA.UnitTests.BioSCADA.ServerComponents.Timers;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers
{

    public class when_testing_BaseDriver : Specification
    {
    }

    public class and_testing_sampling_time : when_testing_BaseDriver
    {
        private AllValuesTestingDriver driver;
        private TimerStub timerStub;
        private VariableStorageStub variableStorage;

        [SetUp]
        public void Setup()
        {
            driver = new AllValuesTestingDriver { ValueToPost = 3.14 };
            timerStub = new TimerStub();
            variableStorage = new VariableStorageStub();
            driver.Timer = timerStub;
            driver.ValueStorage = variableStorage;
            driver.AddVariable(new IntegerVariable(1, "Var1") { TicksToSample = 1 });
            driver.AddVariable(new IntegerVariable(2, "Var2") { TicksToSample = 2 });
            driver.AddVariable(new IntegerVariable(5, "Var5") { TicksToSample = 5 });
            driver.StartAttendingVariable(1);
            driver.StartAttendingVariable(2);
            driver.StartAttendingVariable(5);
        }

        [Test]
        public void then_can_read_test_samples()
        {
            driver.Start(0);
            timerStub.DoTick();
            CheckValuesInValueStorage(1);
            timerStub.DoTick();
            CheckValuesInValueStorage(2);
            timerStub.DoTick();
            CheckValuesInValueStorage(1);
        }

        [Test]
        public void then_can_read_test_samples_complex()
        {
            int[] counts = new int[] {1, 2, 1, 2, 2, 2, 1, 2, 1, 3, 1, 2};
            driver.Start(0);
            for (int i = 0; i < counts.Length; i++)
            {
                timerStub.DoTick();
                CheckValuesInValueStorage(counts[i]);
            }
        }

        private void CheckValuesInValueStorage(int n)
        {
            for (int i = 0; i < n; i++)
                variableStorage.Dequeue().Item2.ShouldEqual(3.14);
            variableStorage.Dequeue().ShouldBeNull();
        }

        internal class AllValuesTestingDriver : BaseDriver
        {
            public List<Tuple<int, double>> ValuesToWrite = new List<Tuple<int, double>>();
            public double ValueToPost { get; set; }

            protected override void DoProcess(Dictionary<int, bool> variablesToRead)
            {
                foreach (var varId in _variables.Keys)
                    if (variablesToRead[varId])
                        SendValueToStorage(varId, ValueToPost);
            }

            protected override void DoWriteValue(int variableId, double newValue)
            {
                ValuesToWrite.Add(Tuple.Create(variableId, newValue));
            }
        }
    }

    public class and_testing_running : when_testing_BaseDriver
    {
        TestingDriver driver;
        TimerStub timerStub;
        VariableStorageStub variableStorage;

        [SetUp]
        public void Setup()
        {
            driver = new TestingDriver();
            timerStub = new TimerStub();
            variableStorage = new VariableStorageStub();
            driver.Timer = timerStub;
            driver.ValueStorage = variableStorage;
            driver.AddVariable(new IntegerVariable(10, "Var10"));
            driver.AddVariable(new IntegerVariable(8, "Var8"));
        }

        [Test]
        public void then_can_send_data_to_ValueStorage()
        {
            driver.StartAttendingVariable(10);
            driver.ValueToPostToVariableStorage = Tuple.Create(10, 3.1416);

            driver.Start();
            timerStub.DoTick();

            StructuralComparisons.StructuralComparer.Compare(variableStorage.Dequeue(),
                                                             Tuple.Create(10, 3.1416)).ShouldEqual(0);
        }

        [Test]
        public void then_driver_ignores_posting_values_of_unattended_variables()
        {
            driver.StartAttendingVariable(8);
            driver.ValueToPostToVariableStorage = Tuple.Create(10, 3.1416);

            driver.Start();
            timerStub.DoTick();

            variableStorage.Dequeue().ShouldBeNull();
        }



    }

    public class and_testing_attending_variables : when_testing_BaseDriver
    {
        TestingDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new TestingDriver();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot attend to unregistered variable")]
        public void then_fail_attending_unregistered_variable()
        {
            driver.StartAttendingVariable(1);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot stop attending variable not attended: 2")]
        public void then_fails_if_stop_attending_not_attended_variable()
        {
            driver.AddVariable(new IntegerVariable(1, "Var1"));
            driver.AddVariable(new IntegerVariable(2, "Var2"));
            driver.StartAttendingVariable(1);
            driver.StopAttendingVariable(2);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot stop attending variable not attended: 2")]
        public void then_fails_stopping_attending_variable_and_none_is_attended()
        {
            driver.StopAttendingVariable(2);
        }

        
    }

    public class base_tests : when_testing_BaseDriver
    {
        TestingDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new TestingDriver();
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot change VariableStorage while running")]
        public void then_fail_if_ValueStorage_is_changed_while_started()
        {
            driver.ValueStorage = new VariableStorageStub();
            driver.Timer = new TimerStub();
            driver.Start();
            driver.ValueStorage = new VariableStorageStub();
        }


        [Test]
        public void then_can_add_driver_variables()
        {
            int variableId = 17;
            var variable = new IntegerVariable(variableId, "Name") {DriverConfiguration = new object()};
            driver.ValueStorage = new VariableStorageStub();
            driver.Timer = new TimerStub();
            driver.Start();
            driver.Stop();
            driver.AddVariable(variable);
            variable.Driver.ShouldEqual(driver);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot add variable if driver start")]
        public void then_fail_can_add_driver_start_variables()
        {
            int variableId = 17;
            var variable = new IntegerVariable(variableId, "Name") { DriverConfiguration = new object() };
            driver.ValueStorage = new VariableStorageStub();
            driver.Timer = new TimerStub();
            driver.Start();
            driver.AddVariable(variable);
        }

        [Test]
        public void then_can_remove_driver_variables()
        {
            int variableId = 17;
            var variable = new IntegerVariable(variableId, "Name") { DriverConfiguration = new object() };
            driver.ValueStorage = new VariableStorageStub();
            driver.Timer = new TimerStub();
            driver.AddVariable(variable);
            driver.RemoveVarible(variableId).ShouldBeTrue();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot remove variable if driver start")]
        public void then_fail_removing_variable_driver_started()
        {
            int variableId = 17;
            var variable = new IntegerVariable(variableId, "Name") { DriverConfiguration = new object() };
            driver.ValueStorage = new VariableStorageStub();
            driver.Timer = new TimerStub();
            driver.AddVariable(variable);
            driver.Start();
            driver.RemoveVarible(variableId).ShouldBeTrue();
        }

        [Test]
        public void then_can_write_new_variable_value_to_driver()
        {
            int variableID = 19;
            double newValue = 20;

            driver.AddVariable(new IntegerVariable(variableID, "Var"));
            driver.WriteValue(variableID, newValue);
            driver.ValuesToWrite[0].Item1.ShouldEqual(variableID);
            driver.ValuesToWrite[0].Item2.ShouldEqual(newValue);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot write value to unexistent variable: 8")]
        public void then_fail_writing_value_if_variable_does_not_exists()
        {
            driver.WriteValue(8, 0);
        }
    }

    public class and_testing_start_stop_operations : when_testing_BaseDriver
    {
        private TestingDriver driver;
        private TimerStub timerStub;

        [SetUp]
        public void Setup()
        {
            timerStub = new TimerStub();
            driver = new TestingDriver
                         {
                             Timer = timerStub,
                             ValueStorage = new VariableStorageStub()
                         };
            driver.AddVariable(new IntegerVariable(1, "Var1"));
        }

        [Test]
        public void then_can_start_driver()
        {
            driver.Start();
            driver.Started.ShouldBeTrue();
        }
        [Test]
        public void then_driver_doesnt_start_timer_if_not_attending_variables()
        {
            driver.Start();
            timerStub.Started.ShouldBeFalse();
        }
        [Test]
        public void then_driver_start_timer_if_attending_variables()
        {
            driver.StartAttendingVariable(1);
            driver.Start();
            timerStub.Started.ShouldBeTrue();
        }
        [Test]
        public void then_driver_start_timer_if_starts_attending_variable()
        {
            driver.Start();
            timerStub.Started.ShouldBeFalse();
            driver.StartAttendingVariable(1);
            timerStub.Started.ShouldBeTrue();
        }
        [Test]
        public void then_driver_restart_timer_if_already_attending_variable()
        {
            driver.Start();
            driver.StartAttendingVariable(1);
            driver.Stop();
            driver.Start();
            timerStub.Started.ShouldBeTrue();
        }
        [Test]
        public void then_driver_doesnot_restart_if_not_attending_variable()
        {
            driver.Start();
            driver.Stop();
            driver.Start();
            timerStub.Started.ShouldBeFalse();
        }
        [Test]
        public void then_can_stop_driver()
        {
            driver.Start();
            driver.Started.ShouldBeTrue();

            driver.Stop();
            driver.Started.ShouldBeFalse();
        }

        [Test]
        public void then_driver_stop_timer_if_stopped()
        {
            driver.Start();
            driver.StartAttendingVariable(1);
            driver.Stop();
            timerStub.Started.ShouldBeFalse();
        }

        [Test]
        public void then_driver_stop_timer_if_stop_attending_variables()
        {
            driver.StartAttendingVariable(1);
            driver.Start();
            timerStub.Started.ShouldBeTrue();
            driver.StopAttendingVariable(1);
            timerStub.Started.ShouldBeFalse();
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Cannot start driver with timer not assigned")]
        public void then_fail_starting_server_with_timer_not_assigned()
        {
            driver.Timer = null;
            driver.Start();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "Cannot change timer on running driver")]
        public void then_fail_changing_timer_on_running_server()
        {
            driver.Start();
            driver.Timer = new TimerStub();
        }


    }
}



