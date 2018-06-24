using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.Alarms;
using BioSCADA.ServerComponents.DBLogger.Persistence.BusinessServices;
using BioSCADA.ServerComponents.Variables;
using Ionic.Zip;

namespace BioSCADA.ServerComponents.DBLogger
{
    public class VariableLogger : IVariableValuesReceiver
    {
        private const int _systemPriority = int.MaxValue;
        private readonly List<ExperimentLogger> _experimentsLogger;
        public List<ExperimentLogger> ExperimentsLogger
        {
            get { return _experimentsLogger; }
        }

        private readonly List<Experiment> _experiments;
        private DateTime _timeToday;
        public IDateTimeProvider TimeProvider { get; set; }
        public VariableLogger(List<Experiment> experiments)
        {
            TimeProvider = new DateTimeProvider();
            _timeToday = TimeProvider.Now;
            _experiments = experiments;
            _experimentsLogger = new List<ExperimentLogger>();
            foreach (var experiment in experiments)
            {
                _experimentsLogger.Add(new ExperimentLogger(experiment));
            }
        }

        public void ReceiveVariableValues(Dictionary<int, double> variableValue)
        {
            if (_timeToday.Day != TimeProvider.Now.Day)
            {
                foreach (var experiment in _experimentsLogger)
                {
                    if (experiment.Started)
                        experiment.Stop(_systemPriority);
                }
                ZipFile();
                _experimentsLogger.Clear();
                foreach (var experiment in _experiments)
                {
                    ExperimentLogger experimentLogger = new ExperimentLogger(experiment);
                    _experimentsLogger.Add(experimentLogger);
                    experimentLogger.Start(_systemPriority);
                }
                _timeToday = TimeProvider.Now;
            }

            foreach (var experimentLogger in _experimentsLogger)
            {
                experimentLogger.ReceiveNewVariableValues(variableValue);
            }
        }

        private void ZipFile()
        {
            DateTime dateTime = DateTime.Now;
            string fileName = dateTime.Year + "_" + dateTime.Month + "_" + dateTime.Day;
            ZipFile file = new ZipFile(fileName);
            file.AddDirectory("LogDay");
            file.Save("Historic//" + fileName + ".zip");
            foreach (var filepath in Directory.GetFiles("LogDay"))
            {
                File.Delete(filepath);
            }
        }

    }
}
