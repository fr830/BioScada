using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Alarms
{
    public interface IAlarmSensor
    {
        bool Check(double value);
    }

    public class RangeAlarmSensor : IAlarmSensor
    {
        public double MinBound { get; set; }
        public double MaxBound { get; set; }

        public bool Check(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Cannot check alarm on NaN value");
            return value < MinBound || value > MaxBound;
        }
    }
}
