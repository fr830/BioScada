using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents
{
    public class ValueStorage : IValueStorage
    {
        private ConcurrentQueue<Tuple<int, double>> _queue = new ConcurrentQueue<Tuple<int, double>>();

        public void Enqueue(int id, double value)
        {
            _queue.Enqueue(new Tuple<int, double>(id, value));
        }

        public Tuple<int, double> Dequeue()
        {
            Tuple<int, double> result;
            if (_queue.TryDequeue(out result))
                return result;
            else
                return null;
        }
    }
}
