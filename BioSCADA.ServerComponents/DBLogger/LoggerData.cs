using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.DBLogger.Persistence;

namespace BioSCADA.ServerComponents.DBLogger
{
    public class LoggerData : Entity<int>
    {
        public virtual int VariableId { get; set; }
        public virtual double VariableValue { get; set; }
        public virtual DateTime TimeStorage { get; set; }
    }
}
