using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Variables;
using BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents
{
    public class when_testing_experiments : Specification
    {

    }

    public class and_testing_security : when_testing_experiments
    {
        private const int varID = 12;
        private Experiment exp;
        private Variable variable;
        
        [SetUp]
        public void Setup()
        {
            exp = new Experiment()
                      {
                          Name = "Exp1", 
                          MinUserLevelToInteract = 20,
                          MinUserLevelToStart = 30,
                          MinUserLevelToStop = 40,
                      };
            variable = new IntegerVariable(varID, "Var1") {Driver = new DriverStub()};
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "Cannot add variable. User have not enough permissions.")]
        public void then_fail_adding_variable_with_lower_permission()
        {
            exp.AddVariable(variable);
        }
        
        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "Cannot remove variable. User have not enough permissions.")]
        public void then_fail_removing_variable_with_lower_permission()
        {
            exp.AddVariable(variable, 20);
            exp.RemoveVariable(varID);
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "Cannot enumerate variable. User have not enough permissions.")]
        public void then_fail_enumerating_variable_with_lower_permission()
        {
            exp.AddVariable(variable, 20);
            exp.GetVariables();
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "Cannot access variable. User have not enough permissions.")]
        public void then_fail_getting_variable_with_lower_permission()
        {
            exp.AddVariable(variable, 20);
            var x = exp[varID];
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "Cannot start experiment. User have not enough permissions.")]
        public void then_fail_starting_experiment_with_lower_permission()
        {
            exp.Start();
        }

        [Test]
        [ExpectedException(typeof(SecurityException), ExpectedMessage = "Cannot stop experiment. User have not enough permissions.")]
        public void then_fail_stopping_experiment_with_lower_permission()
        {
            exp.Start(100);
            exp.Stop();
        }


    }

    public class and_testing_experiment_variables: when_testing_experiments
    {
        private const int varID = 12;
        private const string varName = "Var12";
        private Experiment exp;
        private Variable variable;
        private IDriver driver;

        [SetUp]
        public void Setup()
        {
            exp = new Experiment() { Name = "Exp1" };
            driver = new DriverStub();
            variable = new IntegerVariable(varID, varName) {Driver = driver };
        }

        [Test]
        public void then_can_add_variable_to_experiment()
        {
            exp.AddVariable(variable);
            exp.GetVariables().Count().ShouldEqual(1);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Dupplicate variable name: Var12")]
        public void then_fail_if_adding_variable_with_dupplicate_name()
        {
            exp.AddVariable(variable);
            exp.AddVariable(new IntegerVariable(23, variable.Name){Driver = driver});
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot add variable with empty Name")]
        public void then_fail_if_adding_variable_with_empty_name()
        {
            variable.Name = "";
            exp.AddVariable(variable);
        }

        [Test]
        public void then_can_delete_variable_from_experiment()
        {
            exp.AddVariable(variable);
            exp.RemoveVariable(variable.ID);
            exp.GetVariables().ShouldBeEmpty();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot remove unexisting variable")]
        public void then_cannot_remove_unexisting_variable()
        {
            exp.AddVariable(variable);
            exp.RemoveVariable(23);
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Experiment already started and not add variable")]
        public void then_fail_if_add_variable_started_experiment()
        {
            exp.Start(100);
            exp.AddVariable(variable);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Experiment already started and not remove variable")]
        public void then_fail_if_remove_variable_started_experiment()
        {
            exp.AddVariable(variable);
            exp.Start(100);
            exp.RemoveVariable(variable.ID);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot add variable with unassigned Driver")]
        public void then_fail_adding_variable_with_unassigned_driver()
        {
            exp.AddVariable(new IntegerVariable(1, "Var1"));
        }

        [Test]
        public void then_can_access_variable_by_ID()
        {
            exp.AddVariable(variable);
            exp[varID].ShouldEqual(variable);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Unknown variable: ID=13")]
        public void then_fail_accessing_unexisting_variable()
        {
            exp.AddVariable(variable);
            var x = exp[varID + 1];
        }
        
    }

    public class and_testing_interaction_with_drivers : when_testing_experiments
    {
        private Experiment exp;
        private DriverStub driver;

        [SetUp]
        public void SetUp()
        {
            exp = new Experiment { Name = "Exp1" };
            driver = new DriverStub();
            Variable var1 = new IntegerVariable(1, "Var1") { Driver = driver };
            Variable var2 = new IntegerVariable(2, "Var2") { Driver = driver };
            Variable var3 = new IntegerVariable(3, "Var3") { Driver = driver };
            exp.AddVariable(var1);
            exp.AddVariable(var2);
            exp.AddVariable(var3);
        }

        [Test]
        public void then_starting_experiment_warn_driver()
        {
            exp.Start(100);
            driver.AttendedVariables.ShouldBeEquivalentTo(new List<int>{1, 2, 3});
        }

        [Test]
        public void then_stopping_experiment_warn_driver()
        {
            exp.Start(100);
            driver.AttendedVariables.ShouldBeEquivalentTo(new List<int>{1, 2, 3});
            exp.Stop(100);
            driver.AttendedVariables.ShouldBeEmpty();
        }

    }
}
