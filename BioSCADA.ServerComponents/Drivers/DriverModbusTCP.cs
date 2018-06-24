using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.Drivers
{
    public class DriverModbusTCP : BaseDriver,IDisposable
    {
        private MasterTCP tcp;
        public DriverModbusTCP(string ip, int port)
        {
            tcp = new MasterTCP(ip, port);
        }

        protected override void DoProcess(Dictionary<int, bool> variablesToRead)
        {
            // 1. Cambiar valores
            // 2. Leer valores
            Tuple<int, double> element;
            while (ValuesToWrite.TryDequeue(out element) && element != null)
            {
                if (((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.Coils)
                        || ((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.DiscreteInputs
                        || ((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.SingleCoil)))
                    tcp.WriteSingleCoils((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Station.ID, (_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Address, Convert.ToBoolean(element.Item2));
                else
                {
                    tcp.WriteSingleRegister((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Station.ID, (_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Address, new byte[] { (byte)element.Item2 });
                }
            }

            List<int> varsToRead = (from v in variablesToRead
                                    where v.Value
                                    select v.Key).ToList();
            if (varsToRead.Count > 0)
            {
                List<Variable> vars = new List<Variable>();
                for (int i = 0; i < varsToRead.Count; i++)
                {
                    if (_variables[varsToRead[i]].DriverConfiguration == null)
                        throw new InvalidOperationException(string.Format(
                            "DriverConfiguration not assigned, variable: {0}", varsToRead[i]));
                    vars.Add(_variables[varsToRead[i]]);
                }
                foreach (var variable in vars)
                {
                    if (((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.Coils)
                        || ((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.DiscreteInputs
                        || ((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).TypeData == ModbusTypeData.SingleCoil)))
                    {
                        byte[] value = null;
                        tcp.ReadCoils((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Station.ID, (_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Address, 1, ref value);
                        if (value != null)
                            SendValueToStorage(variable.ID, value[0]);
                    }
                    else
                    {
                        byte[] value = null;
                        tcp.ReadDiscreteInputs((_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Station.ID, (_variables[element.Item1].DriverConfiguration as ModbusConfiguration).Address, 1, ref value);
                        if (value != null)
                            SendValueToStorage(variable.ID, value[0]);
                    }
                }
            }
        }

        ConcurrentQueue<Tuple<int, double>> ValuesToWrite = new ConcurrentQueue<Tuple<int, double>>();
        protected override void DoWriteValue(int variableId, double newValue)
        {
            ValuesToWrite.Enqueue(new Tuple<int, double>(variableId, newValue));
        }

        internal class MasterTCP
        {
            private const byte fctReadCoil = 1;
            private const byte fctReadDiscreteInputs = 2;
            private const byte fctReadHoldingRegister = 3;
            private const byte fctReadInputRegister = 4;
            private const byte fctWriteSingleCoil = 5;
            private const byte fctWriteSingleRegister = 6;
            private const byte fctWriteMultipleCoils = 15;
            private const byte fctWriteMultipleRegister = 16;

            /// <summary>Constant for exception illegal function.</summary>
            public const byte excIllegalFunction = 1;
            /// <summary>Constant for exception illegal data address.</summary>
            public const byte excIllegalDataAdr = 2;
            /// <summary>Constant for exception illegal data value.</summary>
            public const byte excIllegalDataVal = 3;
            /// <summary>Constant for exception slave device failure.</summary>
            public const byte excSlaveDeviceFailure = 4;
            /// <summary>Constant for exception acknoledge.</summary>
            public const byte excAck = 5;
            /// <summary>Constant for exception memory parity error.</summary>
            public const byte excMemParityErr = 6;
            /// <summary>Constant for exception gate path unavailable.</summary>
            public const byte excGatePathUnavailable = 10;
            /// <summary>Constant for exception not connected.</summary>
            public const byte excExceptionNotConnected = 253;
            /// <summary>Constant for exception connection lost.</summary>
            public const byte excExceptionConnectionLost = 254;
            /// <summary>Constant for exception response timeout.</summary>
            public const byte excExceptionTimeout = 255;

            private const byte fctExceptionOffset = 128;

            private static int _timeout = 500;
            private static bool _connected = false;
            private TcpClient tcpAsyClient;
            private TcpClient tcpSynClient;
            private ListenClass thrAsyListen;
            private ListenClass thrSynListen;

            public delegate void ResponseData(int id, byte function, byte[] data);
            public event ResponseData OnResponseData;

            public delegate void ExceptionData(int id, byte function, byte exception);
            public event ExceptionData OnException;

            public int timeout
            {
                get { return _timeout; }
                set { _timeout = value; }
            }

            public bool connected
            {
                get { return _connected; }
            }

            public MasterTCP()
            {
            }

            public MasterTCP(string ip, int port)
            {
                connect(ip, port);
            }

            public void connect(string ip, int port)
            {
                try
                {
                    // ----------------------------------------------------------------
                    // Connect asynchronous client
                    tcpAsyClient = new TcpClient(ip, port);
                    tcpAsyClient.ReceiveBufferSize = 256;
                    tcpAsyClient.SendBufferSize = 256;
                    // ----------------------------------------------------------------
                    // Connect synchronous client
                    tcpSynClient = new TcpClient(ip, port);
                    tcpSynClient.ReceiveBufferSize = 256;
                    tcpSynClient.SendBufferSize = 256;
                    _connected = true;
                }
                catch (System.IO.IOException error)
                {
                    _connected = false;
                    throw (error);
                }
            }

            public void disconnect()
            {
                Dispose();
            }


            ~MasterTCP()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (tcpAsyClient != null)
                {
                    tcpAsyClient.Close();
                    tcpAsyClient = null;
                }
                if (tcpSynClient != null)
                {
                    tcpSynClient.Close();
                    tcpSynClient = null;
                }
                if (thrAsyListen != null)
                {
                    thrAsyListen = null;
                }
                if (thrSynListen != null)
                {
                    thrSynListen = null;
                }
            }

            public void ReadCoils(int id, int startAddress, byte numInputs)
            {
                WriteAsyncData(CreateReadHeader(id, startAddress, numInputs, fctReadCoil), id);
            }

            public void ReadCoils(int id, int startAddress, byte numInputs, ref byte[] values)
            {
                values = WriteSyncData(CreateReadHeader(id, startAddress, numInputs, fctReadCoil), id);
            }

            public void ReadDiscreteInputs(int id, int startAddress, byte numInputs)
            {
                WriteAsyncData(CreateReadHeader(id, startAddress, numInputs, fctReadDiscreteInputs), id);
            }

            public void ReadDiscreteInputs(int id, int startAddress, byte numInputs, ref byte[] values)
            {
                values = WriteSyncData(CreateReadHeader(id, startAddress, numInputs, fctReadDiscreteInputs), id);
            }

            public void ReadHoldingRegister(int id, int startAddress, byte numInputs)
            {
                WriteAsyncData(CreateReadHeader(id, startAddress, numInputs, fctReadHoldingRegister), id);
            }

            public void ReadHoldingRegister(int id, int startAddress, byte numInputs, ref byte[] values)
            {
                values = WriteSyncData(CreateReadHeader(id, startAddress, numInputs, fctReadHoldingRegister), id);
            }

            public void ReadInputRegister(int id, int startAddress, byte numInputs)
            {
                WriteAsyncData(CreateReadHeader(id, startAddress, numInputs, fctReadInputRegister), id);
            }

            public void ReadInputRegister(int id, int startAddress, byte numInputs, ref byte[] values)
            {
                values = WriteSyncData(CreateReadHeader(id, startAddress, numInputs, fctReadInputRegister), id);
            }

            public void WriteSingleCoils(int id, int startAddress, bool OnOff)
            {
                byte[] data;
                data = CreateWriteHeader(id, startAddress, 1, 1, fctWriteSingleCoil);
                if (OnOff == true) data[10] = 255;
                else data[10] = 0;
                WriteAsyncData(data, id);
            }

            public void WriteSingleCoils(int id, int startAddress, bool OnOff, ref byte[] result)
            {
                byte[] data;
                data = CreateWriteHeader(id, startAddress, 1, 1, fctWriteSingleCoil);
                if (OnOff == true) data[10] = 255;
                else data[10] = 0;
                result = WriteSyncData(data, id);
            }

            public void WriteMultipleCoils(int id, int startAddress, int numBits, byte[] values)
            {
                byte numBytes = Convert.ToByte(values.Length);
                byte[] data;
                data = CreateWriteHeader(id, startAddress, numBits, (byte)(numBytes + 2), fctWriteMultipleCoils);
                Array.Copy(values, 0, data, 13, numBytes);
                WriteAsyncData(data, id);
            }

            public void WriteMultipleCoils(int id, int startAddress, int numBits, byte[] values, byte[] result)
            {
                byte numBytes = Convert.ToByte(values.Length);
                byte[] data;
                data = CreateWriteHeader(id, startAddress, numBits, (byte)(numBytes + 2), fctWriteMultipleCoils);
                Array.Copy(values, 0, data, 13, numBytes);
                result = WriteSyncData(data, id);

            }

            public void WriteSingleRegister(int id, int startAddress, byte[] values)
            {
                byte[] data;
                data = CreateWriteHeader(id, startAddress, 1, 1, fctWriteSingleRegister);
                data[10] = values[0];
                data[11] = values[1];
                WriteAsyncData(data, id);
            }

            public void WriteSingleRegister(int id, int startAddress, byte[] values, byte[] result)
            {
                byte[] data;
                data = CreateWriteHeader(id, startAddress, 1, 1, fctWriteSingleRegister);
                data[10] = values[0];
                data[11] = values[1];
                result = WriteSyncData(data, id);
            }

            public void WriteMultipleRegister(int id, int startAddress, int numRegs, byte[] values)
            {
                byte numBytes = Convert.ToByte(values.Length);
                byte[] data;
                data = CreateWriteHeader(id, startAddress, numRegs, (byte)(numBytes + 2), fctWriteMultipleRegister);
                Array.Copy(values, 0, data, 13, numBytes);
                WriteAsyncData(data, id);
            }

            public void WriteMultipleRegister(int id, int startAddress, int numRegs, byte[] values, byte[] result)
            {
                byte numBytes = Convert.ToByte(values.Length);
                byte[] data;
                data = CreateWriteHeader(id, startAddress, numRegs, (byte)(numBytes + 2), fctWriteMultipleRegister);
                Array.Copy(values, 0, data, 13, numBytes);
                result = WriteSyncData(data, id);
            }

            private byte[] CreateReadHeader(int id, int startAddress, byte length, byte function)
            {
                byte[] data = new byte[12];

                byte[] _id = BitConverter.GetBytes((short)id);
                data[0] = _id[0];				// Slave id high byte
                data[1] = _id[1];				// Slave id low byte
                data[5] = 6;					// Message size
                data[6] = 0;					// Slave address
                data[7] = function;				// Function code
                byte[] _adr = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)startAddress));
                data[8] = _adr[0];				// Start address
                data[9] = _adr[1];				// Start address
                data[11] = length;				// Number of data to read
                return data;
            }

            private byte[] CreateWriteHeader(int id, int startAddress, int numData, byte numBytes, byte function)
            {
                byte[] data = new byte[numBytes + 11];

                byte[] _id = BitConverter.GetBytes((short)id);
                data[0] = _id[0];				// Slave id high byte
                data[1] = _id[1];				// Slave id low byte+
                byte[] _size = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)(5 + numBytes)));
                data[4] = _size[0];				// Complete message size in bytes
                data[5] = _size[1];				// Complete message size in bytes
                data[6] = 0;					// Slave address
                data[7] = function;				// Function code
                byte[] _adr = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)startAddress));
                data[8] = _adr[0];				// Start address
                data[9] = _adr[1];				// Start address
                if (function >= fctWriteMultipleCoils)
                {
                    byte[] cnt = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)numData));
                    data[10] = cnt[0];			// Number of bytes
                    data[11] = cnt[1];			// Number of bytes
                    data[12] = (byte)(numBytes - 2);
                }
                return data;
            }

            private void WriteAsyncData(byte[] write_data, int id)
            {
                // --------------------------------------------------------------------
                // Create new asynchronous class
                thrAsyListen = new ListenClass();
                thrAsyListen.OnException += thrListen_OnException;
                thrAsyListen.OnResponseData += thrListen_OnResponseData;
                thrAsyListen.WriteData(tcpAsyClient, write_data, id, false);
            }

            private byte[] WriteSyncData(byte[] write_data, int id)
            {
                // --------------------------------------------------------------------
                // Wait until sync thread is free
                int time = 0;
                while ((thrSynListen != null) && (time < _timeout))
                {
                    Thread.Sleep(10);
                    time += 10;
                }
                // --------------------------------------------------------------------
                // Create new synchronous task
                thrSynListen = new ListenClass();
                thrSynListen.OnException += thrListen_OnException;
                // --------------------------------------------------------------------
                // Response result if write request was ok
                if (thrSynListen.WriteData(tcpSynClient, write_data, id, true))
                {
                    byte[] resp_data = thrSynListen.resp_data;
                    thrSynListen = null;
                    return resp_data;
                }
                // --------------------------------------------------------------------
                // Response null if write request was not ok
                thrSynListen = null;
                return null;
            }

            private void thrListen_OnException(int id, byte function, byte exception)
            {
                if (OnException != null) OnException(id, function, exception);
                if (exception == excExceptionConnectionLost)
                {
                    _connected = false;
                    tcpAsyClient.Close();
                    tcpSynClient.Close();
                    tcpAsyClient = null;
                }
            }

            private void thrListen_OnResponseData(int id, byte function, byte[] data)
            {
                if (OnResponseData != null) OnResponseData(id, function, data);
            }

            private class ListenClass
            {
                internal delegate void ResponseData(int id, byte function, byte[] data);
                internal event ResponseData OnResponseData;
                internal delegate void ExceptionData(int id, byte function, byte exception);
                internal event ExceptionData OnException;

                private TcpClient tcpClient;
                private bool sync = false;
                private int req_id = 0;
                public byte[] resp_data = { };

                public bool WriteData(TcpClient _tcpClient, byte[] write_data, int _req_id, bool _sync)
                {
                    tcpClient = _tcpClient;
                    req_id = _req_id;
                    sync = _sync;
                    // ----------------------------------------------------------------
                    // Send request to slave
                    try
                    {
                        if (_connected)
                        {
                            tcpClient.GetStream().Write(write_data, 0, write_data.Length);
                            Thread thrListen = new Thread(ListenThread);
                            thrListen.Start();
                            if (sync) thrListen.Join();
                            return true;
                        }
                        else if (OnException != null) OnException(req_id, 0, excExceptionNotConnected);
                    }
                    // ----------------------------------------------------------------
                    // Trap connection lost exception
                    catch (Exception error)
                    {
                        if (error.InnerException.GetType() == typeof(System.Net.Sockets.SocketException))
                        {
                            if (OnException != null) OnException(req_id, 0, excExceptionConnectionLost);
                        }
                        else throw (error);
                    }
                    return false;
                }

                private void ListenThread()
                {
                    int time = 0;
                    byte[] buffer = new byte[256];
                    int id = 0;
                    byte[] data;
                    byte function;

                    while (time < _timeout)
                    {
                        // ----------------------------------------------------------------
                        // Wait for new data
                        if ((tcpClient.GetStream().CanRead) &&
                           (tcpClient.GetStream().DataAvailable))
                        {
                            tcpClient.GetStream().Read(buffer, 0, buffer.GetUpperBound(0));
                            id = BitConverter.ToInt16(buffer, 0);
                            function = buffer[7];
                            // ------------------------------------------------------------
                            // Write response data
                            if (function >= fctWriteSingleCoil)
                            {
                                data = new byte[2];
                                Array.Copy(buffer, 10, data, 0, 2);
                            }
                            // ------------------------------------------------------------
                            // Read response data
                            else
                            {
                                data = new byte[buffer[8]];
                                Array.Copy(buffer, 9, data, 0, buffer[8]);
                            }
                            // ------------------------------------------------------------
                            // Response data is slave exception
                            if (function > fctExceptionOffset)
                            {
                                function -= fctExceptionOffset;
                                if (OnException != null) OnException(id, function, buffer[8]);
                            }
                            // ------------------------------------------------------------
                            // Response data is regular data
                            else if ((OnResponseData != null) && (sync == false)) OnResponseData(id, function, data);
                            else resp_data = data;
                            break;
                        }
                        // ----------------------------------------------------------------
                        // Retry reading every 10ms until timeout
                        time += 10;
                        Thread.Sleep(10);
                    }
                    if ((time >= _timeout) && (OnException != null)) OnException(req_id, 0, excExceptionTimeout);
                }
            }
        }

        public void Dispose()
        {
            tcp.disconnect();
        }
    }
}
