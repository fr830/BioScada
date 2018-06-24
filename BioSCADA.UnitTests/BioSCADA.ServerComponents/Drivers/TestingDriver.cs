using System;
using System.Collections.Generic;
using BioSCADA.ServerComponents.Drivers;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers
{
    internal class TestingDriver : BaseDriver
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
}