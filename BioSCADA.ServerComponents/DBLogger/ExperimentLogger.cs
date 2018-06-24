using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.Timers;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.DBLogger
{
    public class ExperimentLogger : ActiveObject, IDisposable
    {
        private const int _systemPriority = int.MaxValue;
        private readonly Experiment _experiment;
        private List<Variables.Variable> _VarsToSave;
        public ExperimentLogger(Experiment experiment)
        {
            _experiment = experiment;
            _VarsValueToSave = new Dictionary<int, double>();
            _experiment.OnStoppedExperiment += OnStopped;
            _experiment.OnStartedExperiment += OnStarted;
            _VarsToSave = new List<Variable>();
            foreach (var variable in experiment.GetVariables(_systemPriority))
            {
                if (variable.LogValues)
                    _VarsToSave.Add(variable);
            }

        }

        private ITimer _timer;

        private void TimerTickHandler()
        {
            DoProcess();
        }

        private void DoProcess()
        {
            using (BitStream stream = new BitStream())
            {
                _experimentSerializer.Serialize(_VarsValueToSave, stream);
                //stream.Position = 0;
                byte[] byteArray = stream.ToByteArray();
                writer.Write(byteArray);
                //stream.CopyTo(fileStream);
            }
            fileStream.Flush(true);
        }

        protected ExperimentSerializer _experimentSerializer;
        private Dictionary<int, double> _VarsValueToSave;
        private FileStream fileStream;
        private BinaryWriter writer;
        protected override void OnStarted()
        {
            //------------------- falta el folder 
            // - Calcular el tick
            // - Calcular nombre del fichero
            Started = true;
            DateTime dateTime = DateTime.Now;
            int tickSeconds = (dateTime.Hour * 3600) + (dateTime.Minute * 60) + dateTime.Second;
            string path = "LogDay//" + _experiment.Name + "_" + (tickSeconds * 4) + ".bin";
            fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite);

            //---------------------------------------------
            _VarsToSave = _experiment.GetVariables(_systemPriority).Where(x => x.LogValues).ToList();
            _VarsValueToSave = _VarsToSave.ToDictionary(x => x.ID, x => 0D);
            //-------Encabezado------------------------------
            MemoryStream varHeaderStream = new MemoryStream();
            BinarySerializer.Serialize(_VarsToSave, varHeaderStream);

            writer = new BinaryWriter(fileStream);
            writer.Write(varHeaderStream.Length);
            writer.Write(varHeaderStream.ToArray());
            fileStream.Flush();
            //---------------------------------
            _experimentSerializer = new ExperimentSerializer(_VarsToSave);

            //--------------Calcular Tick Timer--------------------------------
            _timer = TimerCreator.MCD(_VarsToSave.Select(x => x.TicksToSample).ToList());
            _timer.Tick = TimerTickHandler;
            _timer.Start();
        }

        protected override void OnStopped()
        {
            _timer.Stop();
            fileStream.Close();
        }

        public virtual void ReceiveNewVariableValues(Dictionary<int, double> variableValue)
        {
            foreach (var vars in variableValue)
            {
                if (_VarsValueToSave.ContainsKey(vars.Key))
                    _VarsValueToSave[vars.Key] = vars.Value;
            }
        }

        public void Dispose()
        {
            OnStopped();
        }
    }
}
