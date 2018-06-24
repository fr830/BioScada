using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.Timers;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Timers
{
    public class when_testing_TimerCreatorTests : Specification
    {
        [Test]
        public void when_testing_MCD()
        {
            RealtimeFixedIntervalTimer timer = TimerCreator.MCD(new List<int>() { 3, 4, 8 });
            timer.Interval.ShouldEqual(1*250);
        }

        [Test]
        public void when_testing_MCD1()
        {
            RealtimeFixedIntervalTimer timer = TimerCreator.MCD(new List<int>() { 8, 4, 2, 12 });
            timer.Interval.ShouldEqual(2*250);
        }
    }
}
