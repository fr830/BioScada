using System;
using System.Runtime.Serialization;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;

namespace BioSCADA.ServerComponents.Variables
{
    [Serializable]
    [DataContract]
    public abstract class Variable
    {
        public int ID { get; set; }

        [NonSerialized()]
        private IDriver _driver;
        public IDriver Driver
        {
            get { return _driver; }
            set { _driver = value; }
        }

        public object DriverConfiguration { get; set; }
        public int MinUserLevelToRead { get; set; }
        public int MinUserLevelToWrite { get; set; }
        public string Name { get; set; }
        public int TicksToSample { get; set; }
        public bool LogValues { get; set; }

        public Variable (int id, string name)
        {
            ID = id;
            Name = name;
            TicksToSample = 1;
        }
    }
}
