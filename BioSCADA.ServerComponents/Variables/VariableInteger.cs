using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Variables
{
    [Serializable]
    public class IntegerVariable : Variable
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public IntegerVariable(int id, string name) : base(id, name)
        {
        }

    }
}
