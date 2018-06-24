using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.Drivers
{
    public class ModbusEncoderDecoder
    {
        public IModbusStationRTU Station { get; private set; }

        private byte[] _message;
        public byte[] Message { get { return _message; } }

        private byte[] _response;
        public byte[] Response { get { return _response; } }


        public ModbusEncoderDecoder(IModbusStationRTU station)
        {
            Station = station;
        }

        public void CreateMessageResponsePackageReadCoils(List<Variable> coils)
        {
            byte[] message = new byte[8];
            int contCoils = ((coils[coils.Count - 1].DriverConfiguration as ModbusConfiguration).Address - (coils[0].DriverConfiguration as ModbusConfiguration).Address) + 1;

            message[0] = Station.ID;
            message[1] = 0x01;
            message[2] = (byte)((coils[0].DriverConfiguration as ModbusConfiguration).Address >> 8);
            message[3] = (byte)(coils[0].DriverConfiguration as ModbusConfiguration).Address;     //out 64
            message[4] = 0;
            message[5] = (byte)contCoils;  //de 8 en 8

            byte responceQuantity;
            if (contCoils % 8 == 0)
                responceQuantity = (byte)(contCoils / 8);
            else
                responceQuantity = (byte)((contCoils / 8) + 1);

            byte[] CRC = new byte[2];
            GetCRC(message, ref CRC);
            message[6] = CRC[0];
            message[7] = CRC[1];

            byte[] response = new byte[5 + responceQuantity];

            ProcessMessages(response, message);
        }

        public void CreateMessageResponsePackageWriteCoil(Variable coil, double newValue)
        {
            byte[] message = new byte[8];
            ushort output_Value;
            if (newValue != 0)
                output_Value = 255;
            else
                output_Value = 0;

            message[0] = Station.ID;
            message[1] = 0x05;

            byte MSB = (byte)((coil.DriverConfiguration as ModbusConfiguration).Address >> 8);
            message[2] = MSB;
            message[3] = (byte)((coil.DriverConfiguration as ModbusConfiguration).Address - (MSB << 8));

            byte MSBR = (byte)(output_Value >> 8);
            if (Station.Bit_Endiang)
            {
                message[4] = (byte)(output_Value - (MSBR << 8));
                message[5] = MSBR;
            }
            else
            {
                message[4] = MSBR;
                message[5] = (byte)(output_Value - (MSBR << 8));
            }

            byte[] CRC = new byte[2];
            GetCRC(message, ref CRC);
            message[6] = CRC[0];
            message[7] = CRC[1];

            byte[] response = new byte[8];
            ProcessMessages(response, message);
        }

        public void CreateMessageResponsePackageReadRegister(Variable register)
        {
            byte[] message = new byte[8];
            message[0] = Station.ID;
            message[1] = 0x04;
            message[2] = (byte)((register.DriverConfiguration as ModbusConfiguration).Address >> 8);
            message[3] = (byte)(register.DriverConfiguration as ModbusConfiguration).Address;     //out 64
            message[4] = 0;
            message[5] = 1;  //de 8 en 8
            byte[] CRC = new byte[2];
            GetCRC(message, ref CRC);
            message[6] = CRC[0];
            message[7] = CRC[1];
            byte[] response = new byte[7];
            ProcessMessages(response, message);
        }

        public void CreateMessageResponsePackageWriteRegister(Variable register, double newValue)
        {
            byte[] message = new byte[8];
            message[0] = Station.ID;
            message[1] = 0x06;

            byte MSB = (byte)((register.DriverConfiguration as ModbusConfiguration).Address >> 8);
            message[2] = MSB;
            message[3] = (byte)((register.DriverConfiguration as ModbusConfiguration).Address - (MSB << 8));

            byte MSBR = (byte)((int)newValue >> 8);

            message[4] = (byte)((int)newValue - (MSBR << 8));
            message[5] = MSBR;

            byte[] CRC = new byte[2];
            GetCRC(message, ref CRC);
            message[6] = CRC[0];
            message[7] = CRC[1];

            byte[] response = new byte[8];
            ProcessMessages(response, message);
        }

        public object GetValueResponseRegisters(byte[] response)
        {
            int localValue = 0;
            localValue = response[3];
            localValue <<= 8;
            localValue += response[4];
            return localValue;
        }

        public bool[] GetValueResponseCoils(byte[] response, int numValue)
        {
            short[] num = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 };
            int value_read = response[numValue];
            List<short> res = new List<short>(); //= new short[response[2]];
            bool[] result = new bool[8];

            if (value_read != 0)
            {
                for (int i = 0; i < num.Length; i++)
                {
                    if (value_read == num[i])
                    {
                        res.Add((short)(i + 1));
                        break;
                    }
                    else
                    {
                        if (value_read < num[i])
                        {
                            res.Add((short)(i));
                            value_read -= num[i - 1];
                            i = -1;
                        }
                    }
                }
            }
            for (int i = 0; i < res.Count; i++)
            {
                result[res[i] - 1] = true;
            }

            return result;
        }

        private void ProcessMessages(byte[] response, byte[] message)
        {
            bool result = ReadWritePort(message, response);
            if (result)
            {
                _message = message;
                _response = response;
            }
            else
            {
                _message = null;
                _response = null;
            }
        }

        private bool ReadWritePort(byte[] message, byte[] response)
        {

            try
            {
                Station.Port.Write(message, 0, message.Length);

                for (int i = 0; i < response.Length; i++)
                {
                    response[i] = (byte)Station.Port.ReadByte();
                }

                if (CheckResponse(response))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Error CRC");
                    return false;
                }

            }
            catch
            {
                Console.WriteLine("Error timeOut Catch ReadPort");
                return false;
            }

        }

        #region CRC Computation
        private void GetCRC(byte[] message, ref byte[] CRC)
        {
            //Function expects a modbus message of any length as well as a 2 byte CRC array in which to 
            //return the CRC values:

            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;

            for (int i = 0; i < (message.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
        }
        #endregion

        #region Check Response
        private bool CheckResponse(byte[] response)
        {

            //Perform a basic CRC check:
            byte[] CRC = new byte[2];
            GetCRC(response, ref CRC);
            if (CRC[0] == response[response.Length - 2] && CRC[1] == response[response.Length - 1])
            {

                return true;
            }
            else
            {

                return false;
            }

        }
        #endregion

        
    }
}
