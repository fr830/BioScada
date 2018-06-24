using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BioSCADA.ServerComponents.Timers
{
    public class RealtimeFixedIntervalTimer : ActiveObject, ITimer
    {
        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set
            {
                if (Started)
                    throw new InvalidOperationException("Cannot modify Interval while timer is running");
                _interval = value;
            }
        }

        private Action _tick;
        public Action Tick
        {
            get { return _tick; }
            set
            {
                if (Started)
                    throw new InvalidOperationException("Cannot modify Tick while timer is running");
                _tick = value;
            }
        }

        private Timer timer;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private static void callback(object state)
        {
            var timer = (RealtimeFixedIntervalTimer)state;

            if (!timer.semaphore.Wait(0))
                return;
            (timer).Tick();
            timer.semaphore.Release();
        }

        protected override void OnStarted()
        {
            if (Interval <= 0)
                throw new InvalidOperationException("Cannot start timer with Interval<=1");
            if (Tick == null)
                throw new InvalidOperationException("Cannot start without assigning Tick");
            timer = new Timer(callback, this, 0, Interval);
        }

        protected override void OnStopped()
        {
            timer.Dispose();
        }
    }
}
