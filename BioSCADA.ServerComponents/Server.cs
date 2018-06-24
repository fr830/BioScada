using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents
{
    public class Server : ActiveObject, IServer
    {
        public static Server GetInstance()
        {
            if (_obj == null)
            {
                _obj = new Server();
            }

            return _obj;
        }

        private static Server _obj;

        public Server()
        {
            _obj = this;
            Experiments = new Dictionary<string, Experiment>();
        }

        public void AddExperiment(Experiment experiment, int userPermissionLevel = 0)
        {
            #region Preconditions
            if (string.IsNullOrEmpty(experiment.Name))
                throw new InvalidOperationException("Cannot add experiment with empty Name");
            if (userPermissionLevel < MinUserLevelToAdmin)
                throw new SecurityException("Cannot add experiment with current user permission");
            if (Started)
                throw new InvalidOperationException("Cannot add experiment to running server");
            if (Experiments.Values.Any(x => x.Name == experiment.Name))
                throw new InvalidOperationException(string.Format("Cannot add experiment with dupplicate name: {0}",
                                                                  experiment.Name));
            #endregion
            Experiments.Add(experiment.Name, experiment);

        }

        private Dictionary<string, Experiment> Experiments { get; set; }

        private IValueStorage _valueStorage;
        public IValueStorage ValueStorage
        {
            get { return _valueStorage; }
            set
            {
                if (Started)
                    throw new InvalidOperationException("Cannot change VariableStorer on running server");
                _valueStorage = value;
            }
        }

        private ITimer _timer;
        public ITimer Timer
        {
            get { return _timer; }
            set
            {
                if (Started)
                    throw new InvalidOperationException("Cannot change timer on running server");
                _timer = value;
            }
        }

        public int MinUserLevelToAdmin { get; set; }

        public int MinUserLevelToStart { get; set; }
        public int MinUserLevelToStop { get; set; }

        public void RemoveExperiment(string experimentName, int userPermissionLevel = 0)
        {
            #region Preconditions
            if (userPermissionLevel < MinUserLevelToAdmin)
                throw new SecurityException("Cannot remove experiment with current user permission");
            if (Started)
                throw new InvalidOperationException("Cannot remove experiment to running server");
            if (!Experiments.ContainsKey(experimentName))
                throw new InvalidOperationException("Cannot remove unexisting experiment");
            #endregion
            Experiments.Remove(experimentName);
        }

        public IEnumerable<Experiment> GetExperiments()
        {
            return Experiments.Values.AsEnumerable();
        }

        public bool WriteVariable(string expName, int varId, double newValue, int userPermissionLevel = 0)
        {
            Experiment exp;
            if (!Experiments.TryGetValue(expName, out exp))
                throw new InvalidOperationException(string.Format("Unknown experiment: {0}", expName));

            var variable = Experiments[expName][varId];
            if (userPermissionLevel < variable.MinUserLevelToWrite)
                return false;

            var driver = variable.Driver;
            if (driver == null)
                throw new InvalidOperationException(string.Format("Variable [{0}] has no driver", varId));

            driver.WriteValue(varId, newValue);
            return true;
        }

        public Experiment this[string expName]
        {
            get
            {
                Experiment exp;
                if (!Experiments.TryGetValue(expName, out exp))
                    throw new InvalidOperationException(string.Format("Cannot access unexistent experiment [{0}]",
                                                                      expName));
                return Experiments[expName];
            }
        }


        private Dictionary<int, double> GetVariableValues(string expName, int userPermissionLevel)
        {
            Dictionary<int, double> result = new Dictionary<int, double>();
            foreach (var variable in Experiments[expName].GetVariables())
            {
                if (userPermissionLevel < variable.MinUserLevelToRead)
                    result.Add(variable.ID, double.NegativeInfinity);
                else
                {
                    double variableValue;
                    if (_variableValues.TryGetValue(variable.ID, out variableValue))
                        result.Add(variable.ID, variableValue);
                    else
                        result.Add(variable.ID, double.NaN);
                }
            }
            return result;
        }
        private const int _systemPermission = 10;
        private void InsertVariableIntern()
        {
            foreach (var exp in Experiments.Values)
                foreach (var variable in exp.GetVariables(_systemPermission))
                {
                    if (!_variables.ContainsKey(variable.ID))
                        _variables.Add(variable.ID, variable);
                }
        }

        private Dictionary<int, double> _variableValues;
        Dictionary<int, Variables.Variable> _variables = new Dictionary<int, Variable>();
        private void DoProcess()
        {
            if (!Started)
                throw new InvalidOperationException("Cannot do process on server stopped");

            Dictionary<int, double> changed = new Dictionary<int, double>();

            var pair = ValueStorage.Dequeue();
            while (pair != null)
            {
                if (!_variables.ContainsKey(pair.Item1))
                    InsertVariableIntern();
                if (_variableValues[pair.Item1] != pair.Item2)
                {
                    if ((_variables[pair.Item1] is DoubleVariable))
                    {
                        if ((Math.Abs(pair.Item2 - _variableValues[pair.Item1]) >
                             (_variables[pair.Item1] as DoubleVariable).MinDeltaToReportChange))
                        {
                            _variableValues[pair.Item1] = pair.Item2;
                            changed[pair.Item1] = pair.Item2;
                        }
                        else
                        {
                            if (double.IsNaN(_variableValues[pair.Item1]))
                            {
                                _variableValues[pair.Item1] = pair.Item2;
                                changed[pair.Item1] = pair.Item2;
                            }
                        }
                    }
                    else
                    {
                        _variableValues[pair.Item1] = pair.Item2;
                        changed[pair.Item1] = pair.Item2;
                    }
                }
                pair = ValueStorage.Dequeue();
            }

            if (OnVariablesChange != null && changed.Count > 0)
                ThreadPool.QueueUserWorkItem(AsyncVariableChange, changed);

            VariablesRequest request;
            while (variableRequests.TryDequeue(out request))
            {
                var variables = GetVariableValues(request.ExperimentName, request.UserPermissionLevel);
                request.Callback.BeginInvoke(variables, null, null);
            }
        }

        private void AsyncVariableChange(object obj)
        {
            Dictionary<int, double> values = (Dictionary<int, double>)obj;
            OnVariablesChange(values);
        }

        protected override void OnStarting(object startData)
        {
            if (startData == null || (int)startData < MinUserLevelToStart)
                throw new SecurityException("User does not have permissions to Start server");
            foreach (IDriver driver in _drivers.Where(x => !x.Started))
                driver.Start(startData);
            _variableValues = new Dictionary<int, double>();
            foreach (var experiment in Experiments)
                foreach (var variable in experiment.Value.GetVariables(int.MaxValue))
                    _variableValues.Add(variable.ID, double.NaN);
        }

        protected override void OnStopping(object stopData)
        {
            if (stopData == null || (int)stopData < MinUserLevelToStop)
                throw new SecurityException("User does not have permissions to Stop server");
            foreach (IDriver driver in _drivers.Where(x => x.Started))
                driver.Stop(stopData);
        }

        protected override void OnStarted()
        {
            if (ValueStorage == null)
                throw new InvalidOperationException("Server has no assigment VariableStorage");
            if (Timer == null)
                throw new InvalidOperationException("Cannot start server with timer not assigned");
            Timer.Tick = DoProcess;
            Timer.Start();
        }

        protected override void OnStopped()
        {
            Timer.Stop();
        }

        private class VariablesRequest
        {
            public string ExperimentName { get; set; }
            public Action<Dictionary<int, double>> Callback { get; set; }
            public int UserPermissionLevel { get; set; }
            public VariablesRequest(string experimentName, Action<Dictionary<int, double>> callback,
                int userPermissionLevel)
            {
                ExperimentName = experimentName;
                Callback = callback;
                UserPermissionLevel = userPermissionLevel;
            }
        }

        private ConcurrentQueue<VariablesRequest> variableRequests = new ConcurrentQueue<VariablesRequest>();

        public bool RequestVariables(string expName, Action<Dictionary<int, double>> callback,
            int userPermissionLevel = 0)
        {
            if (!Started)
                return false;
            if (!Experiments.ContainsKey(expName))
                throw new InvalidOperationException(string.Format("Unknown experiment: {0}", expName));
            variableRequests.Enqueue(new VariablesRequest(expName, callback, userPermissionLevel));
            return true;
        }

        private List<IDriver> _drivers = new List<IDriver>();

        public void AddDriver(IDriver driver)
        {
            if (Started)
                throw new InvalidOperationException("Cannot add driver while server running");
            _drivers.Add(driver);
        }

        public void RemoveDriver(IDriver driver)
        {
            if (Started)
                throw new InvalidOperationException("Cannot remove driver from running server");
            _drivers.Remove(driver);
        }

        public IEnumerable<IDriver> GetDrivers()
        {
            return _drivers.AsEnumerable();
        }

        public event Action<Dictionary<int, double>> OnVariablesChange;
    }
}
