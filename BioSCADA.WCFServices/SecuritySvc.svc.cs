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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SecuritySvc" in code, svc and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SecuritySvc : ISecuritySvc
    {
        private readonly ServerInterop _interop;
        
        public SecuritySvc(ServerInterop interop)
        {
            _interop = interop;
        }

        public SecuritySvc()
        {
            
        }
        
        public bool Login(string name, string pass)
        {
            //User user = new User(){Name = name,Password = pass,UserLever = 20};
            //bool result = _interop.Login(name, pass);
            //_interop.User.SessionId = OperationContext.Current.SessionId;
            //return result;
            return true;
        }

        public void Leave()
        {
            
        }
    }
}
