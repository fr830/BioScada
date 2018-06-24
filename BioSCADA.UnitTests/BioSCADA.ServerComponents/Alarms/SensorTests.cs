using System;
using System.Threading;
using BioSCADA.ServerComponents.Alarms;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Alarms
{
    public class when_testing_sensors : Specification
    {
        private RangeAlarmSensor alarmSensor;
        private double sensorMaxBound = 23.4;

        [SetUp]
        public void Setup()
        {
            alarmSensor = new RangeAlarmSensor
                        {
                            MinBound = double.NegativeInfinity,
                            MaxBound = sensorMaxBound,
                        };
        }

        [Test]
        public void then_launch_alarm_condition_greater()
        {
            alarmSensor.Check(34.6).ShouldBeTrue();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot check alarm on NaN value")]
        public void then_fail_checking_alarm_with_NaN()
        {
            alarmSensor.Check(double.NaN);
        }

        [Test]
        public void then_launch_alarm_condition_lower()
        {
            alarmSensor.MinBound = 12.3;
            alarmSensor.Check(3d).ShouldBeTrue();
        }

        
        [Test]
        public void then_do_nothing_if_not_alarm_condition()
        {
            alarmSensor.Check(21d).ShouldBeFalse();
        }

        [Test]
        public void then_do_nothing_if_not_alarm_condition_below()
        {
            alarmSensor.Check(-1000d).ShouldBeFalse();
        }
   
    }
}
