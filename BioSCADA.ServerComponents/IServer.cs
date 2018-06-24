using System;
using System.Collections.Generic;

namespace BioSCADA.ServerComponents
{
    public interface IServer
    {
        void AddExperiment(Experiment experiment, int userPermissionLevel = 0);
        IValueStorage ValueStorage { get; set; }
        ITimer Timer { get; set; }
        int MinUserLevelToAdmin { get; set; }
        int MinUserLevelToStart { get; set; }
        int MinUserLevelToStop { get; set; }
        bool Started { get; set; }
        void RemoveExperiment(string experimentName, int userPermissionLevel = 0);
        IEnumerable<Experiment> GetExperiments();
        bool WriteVariable(string expName, int varId, double newValue, int userPermissionLevel = 0);
        Experiment this[string expName] { get; }

        bool RequestVariables(string expName, Action<Dictionary<int, double>> callback,
                                              int userPermissionLevel = 0);

        void AddDriver(IDriver driver);
        void RemoveDriver(IDriver driver);
        IEnumerable<IDriver> GetDrivers();
        event Action<Dictionary<int, double>> OnVariablesChange;
        void Start(object startData = null);
        void Stop(object stopData = null);
    }
}