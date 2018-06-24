using System.Collections.Generic;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents
{
    public interface IDriver : IActiveObject
    {
        IValueStorage ValueStorage { get; set; }
        void WriteValue(int variableId, double newValue);
        void AddVariable(Variable variable);
        void StartAttendingVariable(int variable);
        void StopAttendingVariable(int variable);
    }
}