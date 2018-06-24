using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents
{
    public class Experiment : ActiveObject
    {
        public int MinUserLevelToStart { get; set; }

        public Experiment()
        {
            Variables = new Dictionary<int, Variable>();
        }
        public string Name { get; set; }

        private Dictionary<int, Variable> Variables { get; set; }

        public int MinUserLevelToInteract { get; set; }

        public int MinUserLevelToStop { get; set; }

        public void AddVariable(Variable variable, int userPermissionLevel = 0)
        {
            #region Preconditions
            if (string.IsNullOrEmpty(variable.Name))
                throw new InvalidOperationException("Cannot add variable with empty Name");
            if (variable.Driver == null)
                throw new InvalidOperationException("Cannot add variable with unassigned Driver");
            if (userPermissionLevel < MinUserLevelToInteract)
                throw new SecurityException("Cannot add variable. User have not enough permissions.");
            if (Started)
                throw new InvalidOperationException("Experiment already started and not add variable");
            if (Variables.Values.Any(x => x.Name == variable.Name))
                throw new InvalidOperationException(string.Format("Dupplicate variable name: {0}", variable.Name));
            #endregion
            Variables.Add(variable.ID, variable);
        }

        public void RemoveVariable(int id, int userPermissionLevel = 0)
        {
            #region Preconditions
            if (userPermissionLevel < MinUserLevelToInteract)
                throw new SecurityException("Cannot remove variable. User have not enough permissions.");
            if (Started)
                throw new InvalidOperationException("Experiment already started and not remove variable");
            if (!Variables.ContainsKey(id))
                throw new InvalidOperationException("Cannot remove unexisting variable");
            #endregion
            Variables.Remove(id);
        }

        protected override void OnStarting(object startData)
        {
            if (startData == null || ((int)startData) < MinUserLevelToStart)
                throw new SecurityException("Cannot start experiment. User have not enough permissions.");
        }

        protected override void OnStopping(object stopData)
        {
            if (stopData == null || ((int)stopData) < MinUserLevelToStop)
                throw new SecurityException("Cannot stop experiment. User have not enough permissions.");
        }

        public IEnumerable<Variable> GetVariables(int userPermissionLevel = 0)
        {
            #region Preconditions
            if (userPermissionLevel < MinUserLevelToInteract)
                throw new SecurityException("Cannot enumerate variable. User have not enough permissions.");
            #endregion
            return Variables.Values.AsEnumerable();
        }

        public Variable this[int id, int userPermissionLevel = 0]
        {
            get
            {
                #region Preconditions
                if (userPermissionLevel < MinUserLevelToInteract)
                    throw new SecurityException("Cannot access variable. User have not enough permissions.");
                #endregion
                Variable result;
                if (!Variables.TryGetValue(id, out result))
                    throw new InvalidOperationException(string.Format("Unknown variable: ID={0}", id));
                return result;
            }
        }

        protected override void OnStarted()
        {
            foreach (var variable in Variables.Values)
                variable.Driver.StartAttendingVariable(variable.ID);
            if (OnStartedExperiment != null)
                OnStartedExperiment();
        }

        protected override void OnStopped()
        {
            foreach (var variable in Variables.Values)
                variable.Driver.StopAttendingVariable(variable.ID);
            if (OnStoppedExperiment != null)
                OnStoppedExperiment();
        }

        public event Action OnStoppedExperiment;
        public event Action OnStartedExperiment;
    }
}
