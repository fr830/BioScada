using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Variables;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents
{
    public class when_testing_value_storage : Specification
    {
        [Test]
        public void then_can_add_variable_value()
        {
            IValueStorage storage = new ValueStorage();
            Variable variable = new IntegerVariable(16, "Var16");
            double newValue = 14.54;

            storage.Enqueue(variable.ID, newValue);
            Tuple<int, double> read = storage.Dequeue();
            read.Item1.ShouldEqual(variable.ID);
            read.Item2.ShouldEqual(newValue);
        }

        [Test]
        public void then_fail_if_no_variable()
        {
            IValueStorage storage = new ValueStorage();
            Tuple<int, double> var2 = storage.Dequeue();
            var2.ShouldBeNull();
        }
    }
}
