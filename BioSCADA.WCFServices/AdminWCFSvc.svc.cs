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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AdminWCFSvc" in code, svc and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AdminWCFSvc : IAdminWCFSvc
    {
        private readonly ServerInterop _interop;
        private readonly User _user;

        public AdminWCFSvc(ServerInterop interop)
        {
            _interop = interop;
            _user = interop.User;
        }

        public AdminWCFSvc()
        {

        }

        private void ValidateAndLog()
        {

            //if (!(OperationContext.Current.SessionId == _user.SessionId))
            //    throw new NotLoggedException("Not logged user or Session expired, please re-login");
        }

        public bool Start_Server()
        {

            ValidateAndLog();
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
            ValidateAndLog();
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

        public bool Login(string name, string pass)
        {
            throw new NotImplementedException();
        }

        public void Leave()
        {
            throw new NotImplementedException();
        }
    }
}
