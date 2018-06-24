using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Drivers;
using BioSCADA.ServerComponents.Variables;
using Moq;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.Drivers
{
    public class ModbusEncoderDecoderTests : Specification
    {
        private ModbusEncoderDecoder encoderDecoder;
        private Mock<IModbusStationRTU> stationRtu;
        private Mock<ISerialComm> port;

        [SetUp]
        public void Setup()
        {
            stationRtu = new Mock<IModbusStationRTU>(MockBehavior.Strict);
            port = new Mock<ISerialComm>(MockBehavior.Strict);
            stationRtu.SetupGet(x => x.ID).Returns(1);
            stationRtu.SetupGet(x => x.Bit_Endiang).Returns(true);
            port.Setup(x => x.ReadByte()).Returns(asd);

            stationRtu.SetupGet(x => x.Port).Returns(port.Object);

            encoderDecoder = new ModbusEncoderDecoder(stationRtu.Object);

        }

        private int c = 0;
        List<byte> _ints = new List<byte> { 1, 2, 3, 4, 5, 42, 187, 8, 9, 10, 11, 12, 197, 106 };
        int asd()
        {
            return _ints[c++];
        }

        [Test]
        public void when_encoder_message_response_pakage_read_coils()
        {
            c = 0;
            _ints = new List<byte> { 1, 2, 3, 4, 5, 42, 187, 8, 9, 10, 11, 12, 197, 106 };
            List<Variable> varsCoils = new List<Variable>(){
                 new IntegerVariable(1,"Var1"){DriverConfiguration = new ModbusConfiguration(stationRtu.Object, 0, ModbusTypeData.Coils)},
                 new IntegerVariable(2,"Var2"){DriverConfiguration = new ModbusConfiguration(stationRtu.Object, 2, ModbusTypeData.Coils)},
                 new IntegerVariable(3,"Var3"){DriverConfiguration = new ModbusConfiguration(stationRtu.Object, 64, ModbusTypeData.Coils) }
            };
            byte[] messag = new byte[] { 1, 1, 0, 0, 0, 65, 252, 58 };
            port.Setup(x => x.Write(messag, 0, messag.Length));
            encoderDecoder.CreateMessageResponsePackageReadCoils(varsCoils);
            encoderDecoder.Message.SequenceEqual(messag);
            encoderDecoder.Response.SequenceEqual(_ints);
        }


        [Test]
        public void when_encoder_message_response_pakage_read_register()
        {
            c = 0;
            _ints = new List<byte> { 1, 2, 3, 4, 5, 42, 187, 8, 9, 10, 11, 12, 197, 106 };
            var register = new IntegerVariable(1, "Var1")
                               {
                                   DriverConfiguration =
                                       new ModbusConfiguration(stationRtu.Object, 20483, ModbusTypeData.Inputs_Registers)
                               };

            byte[] messag = new byte[] { 1, 4, 80, 3, 0, 1, 208, 202 };
            port.Setup(x => x.Write(messag, 0, messag.Length));
            encoderDecoder.CreateMessageResponsePackageReadRegister(register);
            encoderDecoder.Message.SequenceEqual(messag);
            encoderDecoder.Response.SequenceEqual(_ints);
        }

        [Test]
        public void when_encoder_message_response_pakage_write_register()
        {
            c = 0;
            _ints = new List<byte> { 1, 2, 3, 4, 5, 6, 186, 221, 8, 9, 10, 11, 12, 197, 106 };
            var register = new IntegerVariable(1, "Var1")
            {
                DriverConfiguration =
                    new ModbusConfiguration(stationRtu.Object, 20483, ModbusTypeData.Inputs_Registers)
            };

            byte[] messag = new byte[] { 1, 6, 80, 3, 20, 0, 103, 202 };
            port.Setup(x => x.Write(messag, 0, messag.Length));
            encoderDecoder.CreateMessageResponsePackageWriteRegister(register, 20);
            encoderDecoder.Message.SequenceEqual(messag);
            encoderDecoder.Response.SequenceEqual(_ints);
        }

        [Test]
        public void when_encoder_message_response_pakage_write_coil()
        {
            c = 0;
            _ints = new List<byte> { 1, 2, 3, 4, 5, 6, 186, 221, 8, 9, 10, 11, 12, 197, 106 };
            var coil = new IntegerVariable(1, "Var1")
            {
                DriverConfiguration =
                    new ModbusConfiguration(stationRtu.Object, 20483, ModbusTypeData.Inputs_Registers)
            };

            byte[] messag = new byte[] { 1, 5, 80, 3, 255, 0, 109, 58 };
            port.Setup(x => x.Write(messag, 0, messag.Length));
            encoderDecoder.CreateMessageResponsePackageWriteCoil(coil, 1);
            encoderDecoder.Message.SequenceEqual(messag);
            encoderDecoder.Response.SequenceEqual(_ints);
        }

    }


}
