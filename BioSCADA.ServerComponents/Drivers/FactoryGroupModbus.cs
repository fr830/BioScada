using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.Drivers
{
    public static class FactoryGroupModbus
    {
        public static Dictionary<int, Dictionary<ModbusTypeData, List<Variable>>> CreateGroups(List<Variable> vars)
        {
            Dictionary<int, Dictionary<ModbusTypeData, List<Variable>>> groupRead = 
                new Dictionary<int, Dictionary<ModbusTypeData, List<Variable>>>();
//            Dictionary<ModbusTypeData, List<Variable>> coilDictionary = new Dictionary<ModbusTypeData, List<Variable>>();
//            Dictionary<ModbusTypeData, List<Variable>> regDictionary = new Dictionary<ModbusTypeData, List<Variable>>();
            foreach (var variable in vars.OrderBy(x=>(x.DriverConfiguration as ModbusConfiguration).Address).ToList())
            {
                if (((variable.DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.Coils)
                    || ((variable.DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.DiscreteInputs
                    || ((variable.DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.SingleCoil)))
                    if (groupRead.ContainsKey((variable.DriverConfiguration as ModbusConfiguration).Station.ID))
                    {
                        if (groupRead[(variable.DriverConfiguration as ModbusConfiguration).Station.ID].ContainsKey(ModbusTypeData.Coils))
                            groupRead[(variable.DriverConfiguration as ModbusConfiguration).Station.ID][ModbusTypeData.Coils].Add(variable);
                        else
                            groupRead[(variable.DriverConfiguration as ModbusConfiguration).Station.ID].Add(ModbusTypeData.Coils,new List<Variable>(){variable});
                    }
                    else
                    {
                        groupRead.Add((variable.DriverConfiguration as ModbusConfiguration).Station.ID, new Dictionary<ModbusTypeData, List<Variable>>{{ModbusTypeData.Coils, new List<Variable>{variable}}});
                    }
                else
                {
                    if (groupRead.ContainsKey((variable.DriverConfiguration as ModbusConfiguration).Station.ID))
                    {
                        if (groupRead[(variable.DriverConfiguration as ModbusConfiguration).Station.ID].ContainsKey(ModbusTypeData.Inputs_Registers))
                            groupRead[(variable.DriverConfiguration as ModbusConfiguration).Station.ID][ModbusTypeData.Inputs_Registers].Add(variable);
                        else
                            groupRead[(variable.DriverConfiguration as ModbusConfiguration).Station.ID].Add(ModbusTypeData.Inputs_Registers, new List<Variable>() { variable });
                    }
                    else
                    {
                        groupRead.Add((variable.DriverConfiguration as ModbusConfiguration).Station.ID, new Dictionary<ModbusTypeData, List<Variable>> { { ModbusTypeData.Inputs_Registers, new List<Variable> { variable } } });
                    }
                }
                    
            }
            return groupRead;
        }
    }
}
