using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.DBLogger.Persistence;

namespace BioSCADA.ServerComponents.DBLogger
{
    public class User : Entity
    {
        public virtual string Name { get; set; }
        public virtual string Password { get; set; }
        public virtual int UserLever { get; set; }
    }
}
