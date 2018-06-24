using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.Drivers
{
    public abstract class BaseDriver : ActiveObject, IDriver
    {
        private IValueStorage _valueStorage;
        public IValueStorage ValueStorage
        {
            get { return _valueStorage; }
            set
            {
                if (Started)
                    throw new InvalidOperationException("Cannot change VariableStorage while running");
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
                    throw new InvalidOperationException("Cannot change timer on running driver");
                _timer = value;
            }
        }

        protected abstract void DoProcess(Dictionary<int, bool> variablesToRead);
        protected abstract void DoWriteValue(int variableId, double newValue);

        protected void SendValueToStorage(int varIdx, double value)
        {
            lock (_attendedVariablesLock)
            {
                int count;
                if (_attendedCount.TryGetValue(varIdx, out count))
                    if (count > 0)
                        ValueStorage.Enqueue(varIdx, value);
            }
        }

        public void WriteValue(int variableId, double newValue)
        {
            if (!_variables.ContainsKey(variableId))
                throw new InvalidOperationException(string.Format("Cannot write value to unexistent variable: {0}", variableId));
            DoWriteValue(variableId,newValue);
        }
        
        protected Dictionary<int, Variable> _variables = new Dictionary<int, Variable>();

        public void AddVariable(Variable variable)
        {
            if (Started)
                throw new InvalidOperationException("Cannot add variable if driver start");
            _variables.Add(variable.ID, variable);
            variable.Driver = this;
        }

        public bool RemoveVarible(int id)
        {
            if (Started)
                throw new InvalidOperationException("Cannot remove variable if driver start");
            return _variables.Remove(id);
        }

        private object _attendedVariablesLock = new object();
        private int _attendedVariableCount = 0;
        private Dictionary<int, int> _attendedCount = new Dictionary<int, int>();

        public void StartAttendingVariable(int variable)
        {
            if (!_variables.ContainsKey(variable))
                throw new InvalidOperationException("Cannot attend to unregistered variable");
            lock (_attendedVariablesLock)
            {
                _attendedVariableCount++;
                if (_attendedCount.ContainsKey(variable))
                    _attendedCount[variable]++;
                else
                    _attendedCount.Add(variable, 1);
                if (Started && _attendedVariableCount == 1)
                    Timer.Start();
            }
        }

        public void StopAttendingVariable(int variable)
        {
            lock (_attendedVariablesLock)
            {
                _attendedVariableCount--;
                if (!_attendedCount.ContainsKey(variable) || _attendedCount[variable] == 0)
                    throw new InvalidOperationException(string.Format("Cannot stop attending variable not attended: {0}", variable));
                _attendedCount[variable]--;
                if (_attendedVariableCount == 0)
                    Timer.Stop();
            }
        }

        private void TimerTickHandler()
        {
            foreach (var variable in _variables)
            {
                if (--ticksToRead[variable.Key] == 0)
                {
                    varsToRead[variable.Key] = true;
                    ticksToRead[variable.Key] = variable.Value.TicksToSample;
                }
                else
                    varsToRead[variable.Key] = false;
            }
            
            DoProcess(varsToRead);
        }

        private Dictionary<int, bool> varsToRead;
        private Dictionary<int, int> ticksToRead;

        protected override void OnStarted()
        {
            #region Preconditions
            if (ValueStorage == null)
                throw new InvalidOperationException("Value storage not assigned");
            if (Timer == null)
                throw new InvalidOperationException("Cannot start driver with timer not assigned");
            #endregion
            Timer.Tick = TimerTickHandler;
            varsToRead = new Dictionary<int, bool>();
            ticksToRead = new Dictionary<int, int>();
            foreach (var var in _variables)
            {
                varsToRead.Add(var.Key, false);
                ticksToRead.Add(var.Key, var.Value.TicksToSample);
            }
            lock (_attendedVariablesLock)
                if (_attendedVariableCount > 0)
                    Timer.Start();
        }

        protected override void OnStopped()
        {
            Timer.Stop();
        }
    }
}