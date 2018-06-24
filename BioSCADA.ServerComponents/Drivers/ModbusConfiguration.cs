namespace BioSCADA.ServerComponents.Drivers
{
    public class ModbusConfiguration
    {
        private IModbusStationRTU station;
        public IModbusStationRTU Station { get { return station; } set { station = value; } }

        private int _Address;
        public int Address { get { return _Address; } set { _Address = value; } }

        private ModbusTypeData typeData;
        public ModbusTypeData TypeData { get { return typeData; } set { typeData = value; } }

        public ModbusConfiguration(IModbusStationRTU station, int address, ModbusTypeData typeData)
        {
            this.station = station;
            _Address = address;
            this.typeData = typeData;
        }
    }
}