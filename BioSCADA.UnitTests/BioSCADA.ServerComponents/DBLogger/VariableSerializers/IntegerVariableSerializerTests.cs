using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;
using BioSCADA.ServerComponents.Variables;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger.VariableSerializers
{
    public class when_testing_integer_variable_serializer : Specification
    {

        [Test]
        public void then_can_serialize()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255);
            serializer.Serialize(12, stream);
            stream.Length.ShouldEqual(8);
            stream.ToByteArray()[0].ShouldEqual((byte)12);
        }
       
        [Test]
        public void then_can_deserialize()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255);
            serializer.Serialize(12, stream);
            stream.Position = 0;
            serializer.Deserialize(stream).ShouldEqual(12);
        }

        [Test]
        public void then_can_serialize_value_above_limits()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255);
            serializer.Serialize(260, stream);
            stream.Length.ShouldEqual(8);
            stream.ToByteArray()[0].ShouldEqual((byte)255);
        }

        [Test]
        public void then_can_serialize_value_below_limits()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 0, 255);
            serializer.Serialize(-10, stream);
            stream.Length.ShouldEqual(8);
            stream.ToByteArray()[0].ShouldEqual((byte)0);
        }


        [Test]
        public void then_can_serialize_value_between_300_and_340()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 300, 340);
            serializer.Serialize(320, stream);
            stream.Length.ShouldEqual(6);
            stream.ToByteArray()[0].ShouldEqual((byte)20);
        }

        [Test]
        public void then_can_deserialize_value_between_300_and_340()
        {
            var stream = new BitStream();
            var serializer = CreateSerializer(stream, 300, 340);
            serializer.Serialize(320, stream);
            stream.Position = 0;
            serializer.Deserialize(stream).ShouldEqual(320);
        }

       
        //[Test]
        //public void then_can_save_integer_1_byte_minValue_minus255()
        //{
        //    byte[] allData = new byte[5];
        //    int value = 0x40;
        //    serializer.MinValue = -255;
        //    serializer.MaxValue = 255;
        //    serializer.Serialize(value, allData);
        //    allData[3].ShouldEqual((byte)0x3f);
        //    allData[4].ShouldEqual((byte)0x1);
        //}

        //[Test]
        //public void then_can_deserialize_integer_1_byte_minValue_minus255()
        //{
        //    byte[] allData = new byte[5];
        //    allData[3] = 0x3f;
        //    allData[4] = 0x1;
        //    int value = 0x40;
        //    serializer.MinValue = -255;
        //    serializer.MaxValue = 255;
        //    ((int)serializer.Deserialize(allData)).ShouldEqual(value);

        //}

        //[Test]
        //public void then_can_save_integer_2_bytes()
        //{
        //    byte[] allData = new byte[5];
        //    int value = 0x3020;
        //    serializer.MinValue = 0;
        //    serializer.MaxValue = 0xffff;
        //    serializer.Serialize(value, allData);
        //    allData[3].ShouldEqual((byte)0x20);
        //    allData[4].ShouldEqual((byte)0x30);
        //}

        //[Test]
        //public void then_can_deserialize_integer_2_bytes()
        //{
        //    byte[] allData = new byte[5];
        //    allData[3] = 0x20;
        //    allData[4] = 0x30;
        //    int result = 0x3020;
        //    serializer.MinValue = 0;
        //    serializer.MaxValue = 0xffff;
        //    ((int)serializer.Deserialize(allData)).ShouldEqual(result);

        //}

        //[Test]
        //public void then_can_save_integer_3_bytes()
        //{
        //    byte[] allData = new byte[7];
        //    int value = 0x302036;
        //    serializer.MinValue = 0;
        //    serializer.MaxValue = 0xffffff;
        //    serializer.Serialize(value, allData);
        //    allData[3].ShouldEqual((byte)0x36);
        //    allData[4].ShouldEqual((byte)0x20);
        //    allData[5].ShouldEqual((byte)0x30);
        //}

        //[Test]
        //public void then_can_deserialize_integer_3_bytes()
        //{
        //    byte[] allData = new byte[7];
        //    allData[3] = 0x36;
        //    allData[4] = 0x20;
        //    allData[5] = 0x30;
        //    int result = 0x302036;
        //    serializer.MinValue = 0;
        //    serializer.MaxValue = 0xffffff;
        //    ((int)serializer.Deserialize(allData)).ShouldEqual(result);
        //}

        //[Test]
        //public void then_can_deserialize_integer_4_bytes()
        //{
        //    byte[] allData = new byte[7];
        //    allData[3] = 0x78;
        //    allData[4] = 0x36;
        //    allData[5] = 0x20;
        //    allData[6] = 0x30;
        //    int result = 0x30203678;
        //    serializer.MinValue = 0;
        //    serializer.MaxValue = 0x7fffffff;
        //    ((int)serializer.Deserialize(allData)).ShouldEqual(result);
            
        //}

        private static IntegerVariableSerializer CreateSerializer(BitStream stream, int minValue, int maxValue)
        {
            IntegerVariable variable = new IntegerVariable(0, "intVar")
            {
                MinValue = minValue,
                MaxValue = maxValue,
            };
            var serializer = new IntegerVariableSerializer(variable);
            return serializer;
        }

    }
}
