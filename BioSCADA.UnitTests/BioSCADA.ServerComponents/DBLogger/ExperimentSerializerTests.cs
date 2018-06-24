using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.DBLogger;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;
using BioSCADA.ServerComponents.Variables;
using Moq;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger
{
    public class when_testeing_ExperimentSerializerTests : Specification
    {
        [Test]
        public void then_can_create_serializers()
        {
           
            List<Variable> vars = new List<Variable>()
                                      {
                                          new IntegerVariable(0, "var1") { MinValue = 0, MaxValue = 16 },
                                          new BoolVariable(1, "var2") 
                                      };
            ExperimentSerializer serializer = new ExperimentSerializer(vars);
            serializer.Serializers.Count.ShouldEqual(2);
            serializer.Serializers[0].ShouldBeInstanceOfType(typeof(IntegerVariableSerializer));
            serializer.Serializers[1].ShouldBeInstanceOfType(typeof(BoolVariableSerializer));
        }

        [Test]
        public void then_can_serialize()
        {
            BitStream BitStream = new BitStream();
            List<Variable> vars = new List<Variable>()
                                      {
                                          new IntegerVariable(0, "var1") { MinValue = 0, MaxValue = 16 },
                                          new BoolVariable(1, "var2") 
                                      };
            ExperimentSerializer serializer = new ExperimentSerializer(vars);
            Dictionary<int, double> newValues = new Dictionary<int, double>() { { 0, 8 }, { 1, 1 } };
            serializer.Serialize(newValues, BitStream);
            BitStream.ToByteArray()[0].ShouldEqual((byte)17);
        }

        [Test]
        public void then_can_deserialize()
        {
            BitStream _BitStream = new BitStream();
            List<Variable> vars = new List<Variable>()
                                      {
                                          new IntegerVariable(0, "var1") { MinValue = 0, MaxValue = 16 },
                                          new IntegerVariable(2, "var3") { MinValue = 0, MaxValue = 38 },
                                          new BoolVariable(1, "var2") 
                                      };
            ExperimentSerializer serializer = new ExperimentSerializer(vars);
            Dictionary<int, double> newValues = new Dictionary<int, double>() { { 0, 8 }, { 1, 1 } };
            serializer.Serialize(newValues, _BitStream);
            _BitStream.Position = 0;
            Dictionary<int, double> deserialize = serializer.Deserialize(_BitStream);
            deserialize[0].ShouldEqual(newValues[0]);
            deserialize[1].ShouldEqual(newValues[1]);
        }
    }
}