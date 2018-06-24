using System;
using System.IO;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;
using BioSCADA.ServerComponents.Variables;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger.VariableSerializers
{
    public class when_testing_bool_variable_serializer : Specification
    {
        BoolVariableSerializer serializer;
        private BitStream stream;

        [SetUp]
        public void Setup()
        {
            stream = new BitStream();
            BoolVariable variable = new BoolVariable(0, "boolVar");
            serializer = new BoolVariableSerializer(variable);
        }

        [Test]
        public void then_can_serialize()
        {
            serializer.Serialize(1, stream);
            var byteArray = stream.ToByteArray();
            byteArray[0].ShouldEqual((byte)1);
        }

        [Test]
        public void then_can_deserialize()
        {
            serializer.Serialize(1, stream);
            serializer.Serialize(1, stream);
            serializer.Serialize(0, stream);
            stream.Position = 0;
            serializer.Deserialize(stream).ShouldEqual(1);
            serializer.Deserialize(stream).ShouldEqual(1);
            serializer.Deserialize(stream).ShouldEqual(0);
        }
        
    }
}
