using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Drivers;
using BioSCADA.ServerComponents.Variables;
using BioSCADA.UnitTests.BioSCADA.ServerComponents.Timers;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers
{
    public class when_testing_driver_ModbusRTU : Specification
    {

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "DriverConfiguration not assigned, variable: 12")]
        public void then_fail_reading_variable_without_configuration()
        {
            var driver = new DriverModbusRTU();
            var timerStub = new TimerStub();
            driver.Timer = timerStub;
            driver.ValueStorage = new VariableStorageStub();
            driver.AddVariable(new IntegerVariable(12, "Var12") { Driver = driver, DriverConfiguration = null });
            driver.StartAttendingVariable(12);
            driver.Start();
            timerStub.DoTick();
        }

        //[Test]
        //[ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "DriverConfiguration not assigned, variable: 12")]
        //public void then_fail_reading_variable_without_configuration2()
        //{
        //    var driver = new DriverModbusRTU();
        //    var timerStub = new TimerStub();
        //    driver.Timer = timerStub;
        //    driver.ValueStorage = new VariableStorageStub();
        //    driver.AddVariable(new IntegerVariable(12, "Var12")
        //    {
        //        Driver = driver,
        //        DriverConfiguration = new ModbusConfiguration(
        //            new ModbusStationRTU("Est1", 0, new SerialComm("COM1", 19200, 8, Parity.None, StopBits.One), false),
        //            0,
        //            ModbusTypeData.Coils)
        //    });
        //    driver.StartAttendingVariable(12);
        //    driver.Start();
        //    timerStub.DoTick();
        //}
    }
}
