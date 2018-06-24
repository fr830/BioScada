using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Drivers
{
    public interface ISerialComm
    {
        string PortName { get; set; }
        int BaudRate { get; set; }
        int DataBits { get; set; }
        Parity Parity { get; set; }
        StopBits StopBits { get; set; }
        int ReadBufferSize { get; set; }
        int WriteTimeout { get; set; }
        int ReadTimeout { get; set; }
        string PortStatus { get; set; }
        void DiscardInBuffer();
        void DiscardOutBuffer();
        void Write(byte[] buffer, int offset, int count);
        int ReadByte();
        int Read(byte[] buffer, int offset, int count);
        bool Open();
        bool Close();
    }

    [Serializable]
    public class SerialComm : ISerialComm
    {


        public string PortName { get { return sp.PortName; } set { sp.PortName = value; } }
        public int BaudRate { get { return sp.BaudRate; } set { sp.BaudRate = value; } }
        public int DataBits { get { return sp.DataBits; } set { sp.DataBits = value; } }
        public Parity Parity { get { return sp.Parity; } set { sp.Parity = value; } }
        public StopBits StopBits { get { return sp.StopBits; } set { sp.StopBits = value; } }

        public int ReadBufferSize { get { return sp.ReadBufferSize; } set { sp.ReadBufferSize = value; } }
        public int WriteTimeout { get { return sp.WriteTimeout; } set { sp.WriteTimeout = value; } }
        public int ReadTimeout { get { return sp.ReadTimeout; } set { sp.ReadTimeout = value; } }


        private string portStatus;
        public string PortStatus { get { return portStatus; } set { portStatus = value; } }

        public SerialComm()
        {
            sp = new SerialPort();
        }

        public SerialComm(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            sp = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        }

        public void DiscardInBuffer()
        {
            sp.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            sp.DiscardOutBuffer();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            sp.Write(buffer, offset, count);
        }

        public int ReadByte()
        {
            return sp.ReadByte();
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            return sp.Read(buffer, offset, count);
        }

        #region Open / Close Procedures
        public bool Open()
        {
            //Ensure port isn't already opened:
            if (!sp.IsOpen)
            {

                //These timeouts are default and cannot be editted through the class at this point:
                sp.ReadTimeout = 20;
                sp.WriteTimeout = 20;

                try
                {
                    sp.Open();
                }
                catch (Exception err)
                {
                    portStatus = "Error opening " + sp.PortName + ": " + err.Message;
                    return false;
                }
                portStatus = sp.PortName + " opened successfully";
                return true;
            }
            else
            {
                portStatus = sp.PortName + " already opened";
                return true;
            }
        }
        public bool Close()
        {
            //Ensure port is opened before attempting to close:
            if (sp.IsOpen)
            {
                try
                {
                    sp.Close();
                }
                catch (Exception err)
                {
                    portStatus = "Error closing " + sp.PortName + ": " + err.Message;
                    return false;
                }
                portStatus = sp.PortName + " closed successfully";
                return true;
            }
            else
            {
                portStatus = sp.PortName + " is not open";
                return false;
            }
        }
        #endregion
        [NonSerialized]
        private SerialPort sp;
    }
}
