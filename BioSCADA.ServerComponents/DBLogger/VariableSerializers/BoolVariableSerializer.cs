using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.Variables;
using FluentNHibernate.Mapping;

namespace BioSCADA.ServerComponents.DBLogger.VariableSerializers
{
    public class BoolVariableSerializer : IVariableSerializer
    {
        private readonly BoolVariable _variable;

        public BoolVariableSerializer(BoolVariable variable)
        {
            _variable = variable;
        }

        public void Serialize(double newValue, IBitStream bitStream)
        {
            bitStream.Write(newValue == 0 ? false : true);
        }

        public double Deserialize(IBitStream bitStream)
        {
            bool result;
            bitStream.Read(out result);
            return result ? 1 : 0;
        }

        public int BitLength
        {
            get { return 1; }
        }
    }
}
