using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.DBLogger;

namespace BioSCADA.WCFServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ClientWCFSvc" in code, svc and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWCFSvc : IClientWCFSvc
    {
        private readonly ServerInterop _interop;
        private readonly User _user;

        public ClientWCFSvc(ServerInterop interop)
        {
            _interop = interop;
            _user = interop.User;
        }

        public ClientWCFSvc()
        {
            
        }

        private void ValidateAndLog()
        {
            //if (!(OperationContext.Current.SessionId == _user.SessionId))
            //    throw new NotLoggedException("Not logged user or Session expired, please re-login");
        }

        public bool Start_Experiment(string name)
        {
            ValidateAndLog();
            try
            {
                _interop.StartExperiment(name);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public bool Stop_Experiment(string name)
        {
            ValidateAndLog();
            try
            {
                _interop.StopExperiment(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<int, double> GetVariableExperiment(string name)
        {
            return _interop.GetVariablesExperiment(name);
        }

        public bool SetValueVariable(int VariableId, double VariableValue, string ExperimentName)
        {
            return _interop.SetValueVariable(VariableId, VariableValue, ExperimentName);
        }

        public bool Login(string name, string pass)
        {
            throw new NotImplementedException();
        }

        public void Leave()
        {
            throw new NotImplementedException();
        }

        public bool Start_Server()
        {
            throw new NotImplementedException();
        }

        public bool Stop_Server()
        {
            throw new NotImplementedException();
        }
    }

    public class NotLoggedException : Exception
    {
        public string NotLoggedUserOrSessionExpiredPleaseReLogin { get; set; }

        public NotLoggedException(string notLoggedUserOrSessionExpiredPleaseReLogin)
        {
            NotLoggedUserOrSessionExpiredPleaseReLogin = notLoggedUserOrSessionExpiredPleaseReLogin;
        }
    }
}
