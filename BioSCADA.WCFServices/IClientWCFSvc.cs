using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BioSCADA.WCFServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IClientWCFSvc" in both code and config file together.
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IClientWCFSvc : IAdminWCFSvc
    {
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        bool Start_Experiment(string name);
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        bool Stop_Experiment(string name);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        Dictionary<int, double> GetVariableExperiment(string name);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        bool SetValueVariable(int VariableId, double VariableValue, string ExperimentName);
    }
}
