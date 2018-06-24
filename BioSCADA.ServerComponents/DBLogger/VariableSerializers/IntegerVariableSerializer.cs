using System;
using BioSCADA.ServerComponents.Variables;
using FluentNHibernate.Mapping;

namespace BioSCADA.ServerComponents.DBLogger.VariableSerializers
{
    public class IntegerVariableSerializer : IVariableSerializer
    {
        private readonly IntegerVariable _variable;
        private int _bitLength;

        public IntegerVariableSerializer(IntegerVariable variable)
        {
            _variable = variable;
            _bitLength = LenghtSerializer.GetBitsLengthInteger(variable.MinValue, variable.MaxValue);
        }

        public void Serialize(double newValue, IBitStream bitStream)
        {
            int value = Math.Max(Math.Min((int) newValue, _variable.MaxValue), _variable.MinValue) - _variable.MinValue;
            bitStream.Write(value, 0, _bitLength);
        }

        public double Deserialize(IBitStream bitStream)
        {
            int value;
            bitStream.Read(out value, 0, _bitLength);
            return value + _variable.MinValue;
        }

        public int BitLength
        {
            get { return _bitLength; }
        }
    }
}