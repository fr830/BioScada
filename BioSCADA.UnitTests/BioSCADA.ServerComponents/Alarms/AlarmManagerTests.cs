using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BioSCADA.ServerComponents.Alarms;
using Moq;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Alarms
{
    public class when_testing_alarm_manager : Specification
    {
        private AlarmManager manager;
        private Mock<IAlarmSensor> sensor;
        DateTimeProviderStub timeProvider;
        Dictionary<int, double> okValue;
        Dictionary<int, double> alarmValue;

        [SetUp]
        public void Setup()
        {
            timeProvider = new DateTimeProviderStub();
            manager = new AlarmManager();
            manager.TimeProvider = timeProvider;
            okValue = new Dictionary<int, double> { { 1, 10 } };
            alarmValue = new Dictionary<int, double> { { 1, 20 } };
            sensor = new Mock<IAlarmSensor>(MockBehavior.Strict);
            sensor
                .Setup(x => x.Check(10))
                .Returns(false);
            sensor
                .Setup(x => x.Check(20))
                .Returns(true);
            Alarm alarm = new Alarm
            {
                AlarmID = 2,
                Sensor = sensor.Object,
                VariableID = 1,
            };

            manager.Alarms.Add(alarm);
        }


        [Test]
        public void then_generate_alarm()
        {
            manager.ReceiveVariableValues(alarmValue);

            manager.AlarmEvents.Count.ShouldEqual(1);
            CheckAlarmEvent(manager.AlarmEvents[0], 2, 1, int.MaxValue);
        }

        [Test]
        public void then_ignore_if_no_alarm()
        {
            manager.ReceiveVariableValues(okValue);

            manager.AlarmEvents.Count.ShouldEqual(0);
        }

        [Test]
        public void dos_condiciones_de_alarma_seguidas_generan_una_solaalarma()
        {
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);

            manager.AlarmEvents.Count.ShouldEqual(1);
            CheckAlarmEvent(manager.AlarmEvents[0], 2, 1, int.MaxValue);
        }

        [Test]
        public void si_deja_de_haber_condicion_de_alarma_la_alarma_tiene_fecha_de_terminacion()
        {
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);

            manager.ReceiveVariableValues(okValue);

            manager.AlarmEvents.Count.ShouldEqual(1);
            CheckAlarmEvent(manager.AlarmEvents[0], 2, 1, 2);
        }

        [Test]
        public void si_hay_alarma_despues_no_y_despues_si_entonces_se_crean_2_alarmas()
        {
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);

            manager.ReceiveVariableValues(okValue);

            manager.ReceiveVariableValues(alarmValue);

            manager.AlarmEvents.Count.ShouldEqual(2);
            CheckAlarmEvent(manager.AlarmEvents[0], 2, 1, 2);
            CheckAlarmEvent(manager.AlarmEvents[1], 2, 3, int.MaxValue);
        }

        [Test]
        public void dos_alarmas()
        {
            manager.ReceiveVariableValues(alarmValue);

            manager.ReceiveVariableValues(okValue);

            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);

            manager.AlarmEvents.Count.ShouldEqual(2);
            CheckAlarmEvent(manager.AlarmEvents[0], 2, 1, 2);
            CheckAlarmEvent(manager.AlarmEvents[1], 2, 3, int.MaxValue);
        }

        [Test]
        public void muchas_alarmas()
        {
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);

            manager.ReceiveVariableValues(okValue);
            manager.ReceiveVariableValues(okValue);
            manager.ReceiveVariableValues(okValue);

            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);

            manager.ReceiveVariableValues(okValue);
            manager.ReceiveVariableValues(okValue);

            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);
            manager.ReceiveVariableValues(alarmValue);

            manager.AlarmEvents.Count.ShouldEqual(3);
            CheckAlarmEvent(manager.AlarmEvents[0], 2, 1, 2);
            CheckAlarmEvent(manager.AlarmEvents[1], 2, 3, 4);
            CheckAlarmEvent(manager.AlarmEvents[2], 2, 5, int.MaxValue);
        }



        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "AlarmManager has no assigment TimeProvider")]
        public void then_manager_fail_if_receive_variable_with_time_provider_not_assigned()
        {
            manager.TimeProvider = null;
            manager.ReceiveVariableValues(okValue);
        }

        [Test]
        public void then_can_remove_alarmEvent_with_security()
        {
            var alarmEvent = new AlarmEvent() { AlarmID = 2, StartTime = DateTime.Now, EndTime = DateTime.MaxValue, MinUserLevelToRemove = 10 };
            manager.AlarmEvents.Add(alarmEvent);
            int level = 20;
            manager.RemoveEvent(alarmEvent, level);
            manager.AlarmEvents.Count.ShouldEqual(0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Don't have permission to remove alarm 2")]
        public void then_remove_alarmEvent_fail_if_not_have_permission()
        {
            var alarmEvent = new AlarmEvent() { AlarmID = 2, StartTime = DateTime.Now, EndTime = DateTime.MaxValue, MinUserLevelToRemove = 10 };
            manager.AlarmEvents.Add(alarmEvent);
            manager.RemoveEvent(alarmEvent);
        }

        [Test]
        public void se_necesita_un_cierto_nivel_para_ver_cada_evento()
        {
            var alarmEvent = new AlarmEvent() { AlarmID = 2, StartTime = DateTime.Now, EndTime = DateTime.MaxValue, MinUserLevelToRemove = 30 };
            var alarmEvent1 = new AlarmEvent() { AlarmID = 1, StartTime = DateTime.Now, EndTime = DateTime.MaxValue, MinUserLevelToRemove = 30 };
            var alarmEvent2 = new AlarmEvent() { AlarmID = 3, StartTime = DateTime.Now, EndTime = DateTime.MaxValue, MinUserLevelToRemove = 10 };
            manager.AlarmEvents.Add(alarmEvent);
            manager.AlarmEvents.Add(alarmEvent1);
            manager.AlarmEvents.Add(alarmEvent2);
            int level = 20;
            var alarmEvents = manager.ViewEvents(level);
            alarmEvents.Count.ShouldEqual(1);
        }

        [Test]
        public void se_ignora_un_cambio_de_variable_sobre_una_variable_sin_alarma()
        {
            Dictionary<int, double> newValues = new Dictionary<int, double>
                                                    {
                                                        {1, 23.4},
                                                        {2, 24.4}
                                                    };
            bool paso = false;
            sensor
                .Setup(x => x.Check(23.4))
                .Returns(true);
            sensor
                .Setup(x => x.Check(24.4)).Callback(() => { paso = true; }).Returns(true);
            manager.ReceiveVariableValues(newValues);
            paso.ShouldBeFalse();
        }

        //hay que proteger las variables por una zona critica

        private void CheckAlarmEvent(AlarmEvent alarmEvent, int id, int start, int end)
        {
            alarmEvent.AlarmID.ShouldEqual(id);
            if (start != int.MaxValue)
                alarmEvent.StartTime.Day.ShouldEqual(start);
            else
                alarmEvent.StartTime.ShouldEqual(DateTime.MaxValue);
            if (end != int.MaxValue)
                alarmEvent.EndTime.Day.ShouldEqual(end);
            else
                alarmEvent.EndTime.ShouldEqual(DateTime.MaxValue);
        }


    }

    internal class DateTimeProviderStub : IDateTimeProvider
    {
        private int currentDay = 0;
        public DateTime Now
        {
            get
            {
                currentDay++;
                return new DateTime(2000, 1, currentDay);
            }
        }
    }

}
