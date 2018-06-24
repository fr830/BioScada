using System;
using System.Collections.Generic;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers
{
    public class DriverStub : ActiveObject, IDriver
    {
        public List<int> AttendedVariables = new List<int>();

        public void StartAttendingVariable(int variable)
        {
            if (!AttendedVariables.Contains(variable))
                AttendedVariables.Add(variable);
        }

        public void StopAttendingVariable(int variable)
        {
            AttendedVariables.Remove(variable);
        }

        public IValueStorage ValueStorage { get; set; }

        public void WriteValue(int variableId, double newValue)
        {
            throw new NotImplementedException();
        }

        public void AddVariable(Variable variable)
        {
            throw new NotImplementedException();
        }
    }
}