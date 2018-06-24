using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Drivers;
using BioSCADA.ServerComponents.Variables;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents
{
    public class when_testing_variables : Specification
    {
        [Test]
        public void then_can_create_variable()
        {
            Variable variable = new IntegerVariable(12, "Var12")
                                    {
                                        Driver = new DriverModbusRTU(),
                                        DriverConfiguration = new DriverConfiguration()
                                    };
        }

        
    }
}
