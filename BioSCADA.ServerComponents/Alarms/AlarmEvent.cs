using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BioSCADA.ServerComponents.Alarms
{
    [DataContract]
    public class AlarmEvent
    {
        [DataMember]
        public int AlarmID { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime EndTime { get; set; }

        public int MinUserLevelToRemove { get; set; }
    }
}
