using System;
using System.Collections.Generic;

namespace BioSCADA.ServerComponents
{
    public interface IVariableValuesReceiver
    {
        void ReceiveVariableValues(Dictionary<int, double> variableValue);
    }
}