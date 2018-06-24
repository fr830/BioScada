using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Alarms;

namespace BioSCADA.WCFServices
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    interface IServerService
    {
        [OperationContract(IsInitiating = true)]
        int Login(string name, string pass);

        [OperationContract(IsTerminating = true)]
        void Leave();

        [OperationContract(IsInitiating = false)]
        bool Start_Experiment(string name);
        [OperationContract(IsInitiating = false)]
        bool Stop_Experiment(string name);

        [OperationContract(IsInitiating = false)]
        Dictionary<int, double> GetVariableExperiment(string name);

        [OperationContract(IsInitiating = false)]
        Dictionary<string, Dictionary<string, object>> GetAllVariableExperiment();

        [OperationContract(IsInitiating = false)]
        [ServiceKnownType(typeof(object[]))]
        bool SetValueVariable(string VariableName, object VariableValue, string ExperimentName);

        [OperationContract(IsInitiating = false)]
        [ServiceKnownType(typeof(AlarmEvent))]
        [ServiceKnownType(typeof(object[]))]
        AlarmEvent[] GetAlarms();

    }
}
