using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Drivers.DriversTest
{
    public class DriverInt : BaseDriver
    {
        Random rnd = new Random();
        protected override void DoProcess(Dictionary<int, bool> variablesToRead)
        {
            foreach (var b in variablesToRead)
                if (b.Value)
                    SendValueToStorage(b.Key, rnd.Next(0, 200));
        }

        protected override void DoWriteValue(int variableId, double newValue)
        {
            throw new NotImplementedException();
        }
    }
}
