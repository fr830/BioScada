using System;
using System.Collections.Generic;
using BioSCADA.ServerComponents;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers
{
    public class VariableStorageStub : IValueStorage
    {
        private  Queue<Tuple<int, double>> Vars = new Queue<Tuple<int, double>>();
        
        public void Enqueue(int id, double value)
        {
            Vars.Enqueue(new Tuple<int, double>(id, value));
        }

        public Tuple<int, double> Dequeue()
        {
            if (Vars.Count != 0)
                return Vars.Dequeue();
            else
                return null;
        }
    }
}