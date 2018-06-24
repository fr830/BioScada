using System.Collections.Generic;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.DBLogger
{
    public class ExperimentSerializer
    {
        public ExperimentSerializer(List<Variable> vars)
        {
            _serializers = new Dictionary<int, IVariableSerializer>();
            foreach (var variable in vars)
            {
                _lastValues[variable.ID] = 0;
                if (variable is BoolVariable)
                    _serializers.Add(variable.ID, new BoolVariableSerializer((BoolVariable)variable));
                else
                {
                    if (variable is DoubleVariable)
                        _serializers.Add(variable.ID, new DoubleVariableSerializer((DoubleVariable)variable));
                    else
                    {
                        _serializers.Add(variable.ID, new IntegerVariableSerializer((IntegerVariable)variable));
                    }
                }
            }
        }

        private Dictionary<int, double> _lastValues = new Dictionary<int, double>();

        private Dictionary<int, IVariableSerializer> _serializers;
        public Dictionary<int, IVariableSerializer> Serializers
        {
            get { return _serializers; }
            private set { _serializers = value; }
        }

        public void Serialize(Dictionary<int, double> newValues, IBitStream bitStream)
        {
            foreach (var newValue in newValues)
            {
                if (_lastValues.ContainsKey(newValue.Key))
                    _lastValues[newValue.Key] = newValue.Value;
            }
            foreach (var serializer in _serializers)
            {
                serializer.Value.Serialize(_lastValues[serializer.Key], bitStream);
            }
        }


        public Dictionary<int, double> Deserialize(IBitStream bitStream)
        {
            Dictionary<int, double> result = new Dictionary<int, double>();
            foreach (var serializer in Serializers)
            {
                result[serializer.Key] = serializer.Value.Deserialize(bitStream);
            }
            return result;
        }
    }
}