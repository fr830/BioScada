using System;

namespace BioSCADA.ServerComponents
{
    public interface IValueStorage
    {
        void Enqueue(int id, double value);
        Tuple<int, double> Dequeue();
    }
}
