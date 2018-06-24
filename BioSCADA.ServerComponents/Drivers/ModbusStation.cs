using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Drivers
{
    public interface IModbusStationRTU
    {
        string StationName { get; set; }
        byte ID { get; set; }
        ISerialComm Port { get; set; }
        bool Bit_Endiang { get; set; }
    }

    [Serializable]
    public class ModbusStationRTU : IModbusStationRTU
    {
        private string stationName;
        public string StationName { get { return stationName; } set { stationName = value; } }

        private byte id;
        public byte ID { get { return id; } set { id = value; } }

        private ISerialComm port;
        public ISerialComm Port { get { return port; } set { port = value; } }

        private bool bit_Endiang;
        public bool Bit_Endiang { get { return bit_Endiang; } set { bit_Endiang = value; } }


        public ModbusStationRTU(string stationName, byte id, SerialComm port, bool bit_Endiang)
        {
            this.stationName = stationName;
            this.id = id;
            this.port = port;
            this.bit_Endiang = bit_Endiang;
        }



    }
}
