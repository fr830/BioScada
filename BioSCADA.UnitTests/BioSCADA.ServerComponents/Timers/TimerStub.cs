using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Timers
{
    public class TimerStub : ITimer
    {

        public bool Started { get; set; }

        public void Start(object startData)
        {
            Started = true;
        }
        public void Stop(object stopData)
        {
            Started = false;
        }

        public Action Tick { get; set; }
        public void DoTick()
        {
            if (Started && Tick != null)
                Tick();
        }
    }       

}
