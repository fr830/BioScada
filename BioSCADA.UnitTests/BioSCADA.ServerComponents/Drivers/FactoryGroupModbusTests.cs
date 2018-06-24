using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Drivers;
using BioSCADA.ServerComponents.Variables;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers
{
    public class FactoryGroupModbusTests : Specification
    {
        private List<Variable> vars;

        [SetUp]
        public void Setup()
        {
            SerialComm serialComm = new SerialComm("COM1",19200,8,Parity.None,StopBits.One);    
            SerialComm serialComm1 = new SerialComm("COM2",19200,8,Parity.None,StopBits.One);    
            ModbusStationRTU modbusStationRtu1 = new ModbusStationRTU("Est1",1,serialComm,true);
            ModbusStationRTU modbusStationRtu2 = new ModbusStationRTU("Est2",2,serialComm1,true);
            vars = new List<Variable>()
                       {
                           new IntegerVariable(1,"Var1"){DriverConfiguration = new ModbusConfiguration(modbusStationRtu1,10,ModbusTypeData.Coils)},
                           new IntegerVariable(2,"Var2"){DriverConfiguration = new ModbusConfiguration(modbusStationRtu1,1,ModbusTypeData.Coils)},
                           new IntegerVariable(3,"Var3"){DriverConfiguration = new ModbusConfiguration(modbusStationRtu1,22778,ModbusTypeData.Inputs_Registers)},
                           new IntegerVariable(4,"Var4"){DriverConfiguration = new ModbusConfiguration(modbusStationRtu1,22779,ModbusTypeData.Inputs_Registers)},
                           new IntegerVariable(5,"Var5"){DriverConfiguration = new ModbusConfiguration(modbusStationRtu2,5,ModbusTypeData.Coils)},
                           new IntegerVariable(6,"Var6"){DriverConfiguration = new ModbusConfiguration(modbusStationRtu2,18889,ModbusTypeData.Inputs_Registers)},
                           new IntegerVariable(7,"Var7"){DriverConfiguration = new ModbusConfiguration(modbusStationRtu2,18890,ModbusTypeData.Inputs_Registers)},
                       };
        }

        [Test]
        public void when_create_groups()
        {
            Dictionary<int, Dictionary<ModbusTypeData, List<Variable>>> result =
                new Dictionary<int, Dictionary<ModbusTypeData, List<Variable>>>()
                    {
                        {1, new Dictionary<ModbusTypeData, List<Variable>>()
                                {
                                    {ModbusTypeData.Coils,new List<Variable>(){vars[1],vars[0]}}, 
                                    {ModbusTypeData.Inputs_Registers, new List<Variable>(){vars[2],vars[3]}}
                                }
                         }, 
                         {2, new Dictionary<ModbusTypeData, List<Variable>>()
                                    {
                                        {ModbusTypeData.Coils,new List<Variable>(){vars[4],}},
                                        {ModbusTypeData.Inputs_Registers, new List<Variable>(){vars[5],vars[6]}}
                                    }
                         }
                    };

            Dictionary<int, Dictionary<ModbusTypeData, List<Variable>>> temp = FactoryGroupModbus.CreateGroups(vars);
            temp.SequenceEqual(result);
        }
    }
}
