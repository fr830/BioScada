using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.Drivers
{
    public enum ModbusTypeData
    {
        Coils, DiscreteInputs, Inputs_Registers, MultipleRegisters, SingleCoil, SingleRegister
    }

    public class DriverModbusRTU : BaseDriver
    {
        protected override void DoProcess(Dictionary<int, bool> variablesToRead)
        {
            // 1. Cambiar valores
            // 2. Leer valores
            Tuple<int, double> element;
            while (ValuesToWrite.TryDequeue(out element) && element != null)
            {
                if (((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.Coils)
                        || ((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.DiscreteInputs
                        || ((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.SingleCoil)))
                    stationRtus[(_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Station.ID].CreateMessageResponsePackageWriteCoil(_variables[element.Item1], element.Item2);
                else
                {
                    stationRtus[(_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Station.ID].CreateMessageResponsePackageWriteRegister(_variables[element.Item1], element.Item2);
                }
            }

            List<int> varsToRead = (from v in variablesToRead
                                    where v.Value
                                    select v.Key).ToList();
            if (varsToRead.Count > 0)
            {
                List<Variable> vars = new List<Variable>();
                for (int i = 0; i < varsToRead.Count; i++)
                {
                    if (_variables[varsToRead[i]].DriverConfiguration == null)
                        throw new InvalidOperationException(string.Format(
                            "DriverConfiguration not assigned, variable: {0}", varsToRead[i]));
                    vars.Add(_variables[varsToRead[i]]);
                }
                foreach (var station in FactoryGroupModbus.CreateGroups(vars))
                {
                    if (!stationRtus.ContainsKey(station.Key))
                        stationRtus.Add(station.Key, new ModbusEncoderDecoder((station.Value.Values.First()[0].DriverConfiguration as ModbusConfiguration).Station));
                    List<Variable> Coils = new List<Variable>();
                    if (station.Value.TryGetValue(ModbusTypeData.Coils, out Coils))
                    {
                        ReadCoils(Coils, station.Key);
                    }
                    List<Variable> Registers = new List<Variable>();
                    if (station.Value.TryGetValue(ModbusTypeData.Inputs_Registers, out Registers))
                    {
                        ReadRegisters(Registers, station.Key);
                    }
                }
            }
        }

        private void ReadRegisters(List<Variable> registers, int stationId)
        {
            for (int i = 0; i < registers.Count; i++)
            {
                stationRtus[stationId].CreateMessageResponsePackageReadRegister(registers[i]);
                if (stationRtus[stationId].Response != null)
                    SendValueToStorage(registers[i].ID, Convert.ToDouble(stationRtus[stationId].GetValueResponseRegisters(stationRtus[stationId].Response)));
            }
        }

        Dictionary<int, ModbusEncoderDecoder> stationRtus = new Dictionary<int, ModbusEncoderDecoder>();
        private void ReadCoils(List<Variable> coils, int stationId)
        {

            if (stationRtus[stationId].Station.Port.Open())
            {

                stationRtus[stationId].Station.Port.DiscardOutBuffer();
                stationRtus[stationId].Station.Port.DiscardInBuffer();

                stationRtus[stationId].CreateMessageResponsePackageReadCoils(coils);
                if (stationRtus[stationId].Response != null)
                {
                    byte responceQuantity;
                    int contCoils = stationRtus[stationId].Message[5];
                    if (contCoils % 8 == 0)
                        responceQuantity = (byte)(contCoils / 8);
                    else
                        responceQuantity = (byte)((contCoils / 8) + 1);

                    int startAddress = (coils[0].DriverConfiguration as ModbusConfiguration).Address;
                    for (int i = 0; i < responceQuantity; i++)
                    {
                        bool[] resValue = stationRtus[stationId].GetValueResponseCoils(stationRtus[stationId].Response, 3 + i);

                        for (int j = 0; j < resValue.Length; j++)
                        {
                            if (coils.Exists(x => (x.DriverConfiguration as ModbusConfiguration).Address == startAddress))
                            {
                                var variab = coils.Find(x => (x.DriverConfiguration as ModbusConfiguration).Address == startAddress);
                                SendValueToStorage(variab.ID, Convert.ToDouble(resValue[j]));
                            }
                            startAddress++;
                        }
                    }
                }
            }
        }

        ConcurrentQueue<Tuple<int, double>> ValuesToWrite = new ConcurrentQueue<Tuple<int, double>>();
        protected override void DoWriteValue(int variableId, double newValue)
        {
            ValuesToWrite.Enqueue(new Tuple<int, double>(variableId, newValue));
        }
    }
}
