using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.Alarms;
using BioSCADA.ServerComponents.DBLogger;

namespace BioSCADA.WCFServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ServerService : IClientWCFSvc
    {

        private ServerInterop _interop;

        public ServerService()
        {
            Server server = Server.GetInstance();
            _interop = new ServerInterop(server);
        }

        private User _user;
        public bool Login(string name, string pass)
        {
            if (_interop.Login(name, pass))
            {
                string SessionId = OperationContext.Current.SessionId;
                _user = _interop.User;
                return true;    
            }
            else
            {
                return false;
            }
        }

        public void Leave()
        {

        }

        public bool Start_Experiment(string name)
        {
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
            string SessionId = OperationContext.Current.SessionId;
            return _interop.GetVariablesExperiment(name);
        }

        public bool SetValueVariable(int VariableId, double VariableValue, string ExperimentName)
        {
            return _interop.SetValueVariable(VariableId, VariableValue, ExperimentName);
        }

        public bool Start_Server()
        {
            try
            {
                _interop.StartServer(_user.UserLever);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Stop_Server()
        {
            try
            {
                _interop.StopServer(_user.UserLever);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
