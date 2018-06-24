using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents
{
    public interface IActiveObject
    {
        bool Started { get; set; }
        void Start(object startData = null);
        void Stop(object stopData = null);
    }

    public class ActiveObject : IActiveObject
    {
        public bool Started { get; set; }

        public void Start(object startData = null)
        {
            if (Started)
                throw new InvalidOperationException("Object already started");
            OnStarting(startData);
            Started = true;
            OnStarted();
        }

        public void Stop(object stopData = null)
        {
            if (!Started)
                throw new InvalidOperationException("Object already stopped");
            OnStopping(stopData);
            Started = false;
            OnStopped();
        }

        protected virtual void OnStarting(object startData)
        {
            
        }

        protected virtual void OnStopping(object stopData)
        {
            
        }

        protected virtual void OnStarted()
        {
        }

        protected virtual void OnStopped()
        {
            
        }
    }
}
