using System;
using System.Collections.Generic;

namespace BioSCADA.ServerComponents.Drivers.DriversTest
{
    public class DriverBool : BaseDriver
    {
        Random rnd = new Random();
        protected override void DoProcess(Dictionary<int, bool> variablesToRead)
        {
            foreach (var b in variablesToRead)
                if (b.Value)
                    SendValueToStorage(b.Key, rnd.Next(0, 1));
        }

        protected override void DoWriteValue(int variableId, double newValue)
        {
            throw new NotImplementedException();
        }
    }
}