using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using BioSCADA.IntegrationTests;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Timers;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Timers
{
    public class when_testing_RealtimeFixedIntervalTimerTests : Specification 
    {

        [Test]
        public void then_can_start_timer()
        {
            ITimer timer = new RealtimeFixedIntervalTimer { Interval = 100 };
            object lckTicks = new object();
            int ticks = 0;
            timer.Tick = new Action(() =>
                                        {
                                            lock(lckTicks)
                                                ticks++;
                                        });
            timer.Start();
            Thread.Sleep(1000);
            lock (lckTicks)
            {
                ticks.ShouldBeGreaterThan(8);
                Debug.WriteLine("Ticks: " + ticks);
            }
        }

        [Test]
        public void then_can_stop_timer()
        {
            object lckTicks = new object();
            int ticks = 0;
            ITimer timer = new RealtimeFixedIntervalTimer { Interval = 100 };
            timer.Tick = new Action(() =>
                                        {
                                            lock(lckTicks)
                                                ticks++;
                                        });
            timer.Start();
            Thread.Sleep(100);
            timer.Stop();
            int ticksWhenStop = 0;
            lock (lckTicks)
                ticksWhenStop = ticks;
            Thread.Sleep(1000);
            lock(lckTicks)
                ticks.ShouldEqual(ticksWhenStop);
        }

        [Test]
        public void then_timer_bypass_tick_if_already_running()
        {
            object lckTicks = new object();
            int ticks = 0;
            ITimer timer = new RealtimeFixedIntervalTimer { Interval = 100 };
            timer.Tick = new Action(() =>
            {
                lock (lckTicks)
                    ticks++;
                Thread.Sleep(500);
            });
            timer.Start();
            Thread.Sleep(1000);
            lock (lckTicks)
            {
                ticks.ShouldBeLessThanOrEqualTo(2);
                ticks.ShouldBeGreaterThanOrEqualTo(1);
                Debug.WriteLine("Ticks: " + ticks);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot start timer with Interval<=1")]
        public void then_fail_if_started_with_Interval_below_1()
        {
            ITimer timer = new RealtimeFixedIntervalTimer { Interval = 0 };
            timer.Start();
            Thread.Sleep(10);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot start without assigning Tick")]
        public void then_fail_if_started_without_assigning_Tick()
        {
            ITimer timer = new RealtimeFixedIntervalTimer { Interval = 200 };
            timer.Start();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot modify Interval while timer is running")]
        public void then_fail_if_Interval_changed_when_runnig()
        {
            RealtimeFixedIntervalTimer timer = new RealtimeFixedIntervalTimer
                                                   {
                                                       Interval = 100, 
                                                       Tick = new Action(() => { })
                                                   };
            timer.Start();
            timer.Interval = 200;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot modify Tick while timer is running")]
        public void then_fail_if_Tick_changed_when_runnig()
        {
            RealtimeFixedIntervalTimer timer = new RealtimeFixedIntervalTimer
                                                   {
                                                       Interval = 100, 
                                                       Tick = new Action(() => { })
                                                   };
            timer.Start();
            timer.Tick = new Action(() => {});
        }

    }
}
