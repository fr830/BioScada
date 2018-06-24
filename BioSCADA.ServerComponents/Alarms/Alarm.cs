using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Alarms
{
    public class Alarm
    {
        public int AlarmID { get; set; }

        public IAlarmSensor Sensor { get; set; }

        public int VariableID { get; set; }
    }
}
