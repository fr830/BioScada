using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;
using BioSCADA.ServerComponents.Variables;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger.VariableSerializers
{
    public class when_testing_double_variable_serializer : Specification
    {
        private static DoubleVariableSerializer CreateSerializer(BitStream stream, int minValue, int maxValue, int decimalPlace)
        {
            DoubleVariable variable = new DoubleVariable(0, "DoubleVar")
            {
                MinValue = minValue,
                MaxValue = maxValue,
                DecimalPlaces = decimalPlace
            };
            var serializer = new DoubleVariableSerializer(variable);
            return serializer;
        }

        [Test]
        public void then_can_serialize()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255, 0);
            serializer.Serialize(12, stream);
            stream.Length.ShouldEqual(8);
            stream.ToByteArray()[0].ShouldEqual((byte)12);
        }

        [Test]
        public void then_can_save_double_and_value_is_more_than_maxValue()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255, 0);
            serializer.Serialize(260, stream);
            stream.Length.ShouldEqual(8);
            stream.ToByteArray()[0].ShouldEqual((byte)255);
        }

        [Test]
        public void then_can_save_double_and_value_is_less_than_minValue()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255, 0);
            serializer.Serialize(-1, stream);
            stream.Length.ShouldEqual(8);
            stream.ToByteArray()[0].ShouldEqual((byte)0);
        }

        [Test]
        public void then_can_deserialize_double_1_byte()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255, 0);
            serializer.Serialize(12, stream);
            stream.Position = 0;
            serializer.Deserialize(stream).ShouldEqual(12);

        }

        [Test]
        public void then_can_save_double_3_byte()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255, 3);
            serializer.Serialize(240.235342, stream);
            stream.Length.ShouldEqual(18);
            byte[] byteArray = stream.ToByteArray();
            byteArray[0].ShouldEqual((byte)234);
            byteArray[1].ShouldEqual((byte)154);
            byteArray[2].ShouldEqual((byte)0x3);
        }

        [Test]
        public void then_can_deserialize_double_3_byte()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255, 3);
            serializer.Serialize(240.235342, stream);
            stream.Length.ShouldEqual(18);
            stream.Position = 0;
            serializer.Deserialize(stream).ShouldEqual(240.235);
        }

        [Test]
        public void then_can_save_double_2_byte_2_int()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 50000, 0);
            serializer.Serialize(12358.235342, stream);
            stream.Length.ShouldEqual(16);
            byte[] byteArray = stream.ToByteArray();
            byteArray[0].ShouldEqual((byte)48);
            byteArray[1].ShouldEqual((byte)70);


        }

        [Test]
        public void then_can_deserialize_double_2_byte_2_int()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 50000, 0);
            serializer.Serialize(12358.235342, stream);
            stream.Position = 0;
            serializer.Deserialize(stream).ShouldEqual(12358);
        }

        [Test]
        public void then_can_save_double_3_byte_lenght()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 50000, 2);
            serializer.Serialize(12358.235342, stream);
            stream.Length.ShouldEqual(23);
            byte[] byteArray = stream.ToByteArray();
            byteArray[0].ShouldEqual((byte)37);
            byteArray[1].ShouldEqual((byte)182);
            byteArray[2].ShouldEqual((byte)111);
        }

        [Test]
        public void then_can_deserialize_double_3_byte_lenght()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 50000, 2);
            serializer.Serialize(12358.235342, stream);
            stream.Position = 0;
            serializer.Deserialize(stream).ShouldEqual(12358.23);
        }

        [Test]
        public void then_can_save_double_4_byte_2_int_2_decimal()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 50000, 4);
            serializer.Serialize(12358.235342, stream);
            stream.Length.ShouldEqual(29);
            byte[] byteArray = stream.ToByteArray();
            byteArray[0].ShouldEqual((byte)58);
            byteArray[1].ShouldEqual((byte)237);
            byteArray[2].ShouldEqual((byte)188);
            byteArray[3].ShouldEqual((byte)17);

        }

        [Test]
        public void then_can_deserialize_double_4_byte_2_int_2_decimal()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 50000, 4);
            serializer.Serialize(12358.235342, stream);
            stream.Position = 0;
            serializer.Deserialize(stream).ShouldEqual(12358.2353);
        }
    }
}
