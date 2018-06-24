using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Variables
{
    [Serializable]
    public class DoubleVariable : Variable
    {
        public DoubleVariable(int id, string name)
            : base(id, name)
        {
        }

        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int DecimalPlaces { get; set; }
        public double MinDeltaToReportChange { get; set; }
    }
}
