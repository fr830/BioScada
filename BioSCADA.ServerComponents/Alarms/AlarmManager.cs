using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Alarms
{
    public class AlarmManager : IVariableValuesReceiver
    {
        public IDateTimeProvider TimeProvider { get; set; }
        public List<AlarmEvent> AlarmEvents { get; private set; }
        public List<Alarm> Alarms { get; private set; }

        public static AlarmManager GetInstance()
        {
            if (_obj == null)
            {
                _obj = new AlarmManager();
            }

            return _obj;
        }

        private static AlarmManager _obj;

        public AlarmManager()
        {
            _obj = this;
            Alarms = new List<Alarm>();
            AlarmEvents = new List<AlarmEvent>();
        }

        public void ReceiveVariableValues(Dictionary<int, double> variableValue)
        {
            if (TimeProvider == null)
                throw new InvalidOperationException("AlarmManager has no assigment TimeProvider");
            foreach (Alarm alarm in Alarms)
            {
                double newValue;
                if (variableValue.TryGetValue(alarm.VariableID, out newValue))
                {
                    AlarmEvent alarmExist = AlarmEvents.FindLast(x => x.AlarmID == alarm.AlarmID);
                    if (alarm.Sensor.Check(newValue))
                    {
                        if (alarmExist == null)
                        {
                            var newEvent = new AlarmEvent
                            {
                                AlarmID = alarm.AlarmID,
                                StartTime = TimeProvider.Now,
                                EndTime = DateTime.MaxValue,
                            };
                            AlarmEvents.Add(newEvent);
                        }
                        else
                            if (alarmExist.EndTime != DateTime.MaxValue)
                            {
                                var newEvent = new AlarmEvent
                                {
                                    AlarmID = alarm.AlarmID,
                                    StartTime = TimeProvider.Now,
                                    EndTime = DateTime.MaxValue,
                                };
                                AlarmEvents.Add(newEvent);
                            }
                    }
                    else
                        if (alarmExist != null)
                            if (alarmExist.EndTime == DateTime.MaxValue)
                                alarmExist.EndTime = TimeProvider.Now;
                }
            }
        }

        public void RemoveEvent(AlarmEvent alarmEvent, int levelSecurity = 0)
        {
            if (alarmEvent.MinUserLevelToRemove > levelSecurity)
                throw new ArgumentException(string.Format("Don't have permission to remove alarm {0}", alarmEvent.AlarmID));
            else
                AlarmEvents.Remove(alarmEvent);

        }

        public List<AlarmEvent> ViewEvents(int UserLevel = 0)
        {
            List<AlarmEvent> result = new List<AlarmEvent>();
            foreach (var alarmEvent in AlarmEvents.FindAll(x => x.MinUserLevelToRemove < UserLevel))
            {
                result.Add(new AlarmEvent
                {
                    AlarmID = alarmEvent.AlarmID,
                    EndTime = new DateTime(alarmEvent.EndTime.Year,
                                           alarmEvent.EndTime.Month,
                                           alarmEvent.EndTime.Day,
                                           alarmEvent.EndTime.Hour,
                                           alarmEvent.EndTime.Minute,
                                           alarmEvent.EndTime.Second),
                    StartTime = new DateTime(alarmEvent.StartTime.Year,
                                             alarmEvent.StartTime.Month,
                                             alarmEvent.StartTime.Day,
                                             alarmEvent.StartTime.Hour,
                                             alarmEvent.StartTime.Minute,
                                             alarmEvent.StartTime.Second),
                    MinUserLevelToRemove = alarmEvent.MinUserLevelToRemove
                });
                if (alarmEvent.EndTime != DateTime.MaxValue)
                    AlarmEvents.Remove(alarmEvent);
            }

            return result;
        }
    }
}
