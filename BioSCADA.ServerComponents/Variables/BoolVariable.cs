using System;

namespace BioSCADA.ServerComponents.Variables
{
    [Serializable]
    public class BoolVariable : Variable
    {
        public BoolVariable(int id, string name) : base(id, name)
        {
        }
    }
}