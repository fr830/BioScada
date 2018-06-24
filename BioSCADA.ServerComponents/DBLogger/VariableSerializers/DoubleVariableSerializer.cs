using System;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.DBLogger.VariableSerializers
{
    public class DoubleVariableSerializer : IVariableSerializer
    {
        private readonly DoubleVariable _variable;
        private int _bitLength;

        public DoubleVariableSerializer(DoubleVariable variable)
        {
            _variable = variable;
            _bitLength = LenghtSerializer.GetBitsLengthDouble(variable.MinValue,variable.MaxValue,variable.DecimalPlaces);
        }

        public int BitLength
        {
            get { return _bitLength; }
        }

        public void Serialize(double newValue, IBitStream bitStream)
        {
            int aux = (int)Math.Truncate(newValue * Math.Pow(10, _variable.DecimalPlaces));
            int value = Math.Max(Math.Min(aux, (int)(_variable.MaxValue * Math.Pow(10, _variable.DecimalPlaces))), _variable.MinValue) - _variable.MinValue;
            bitStream.Write(value, 0, _bitLength);
        }

        public double Deserialize(IBitStream bitStream)
        {
            int value;
            bitStream.Read(out value, 0, _bitLength);
            return (value*Math.Pow(10, -_variable.DecimalPlaces)) + _variable.MinValue;
        }
    }
}