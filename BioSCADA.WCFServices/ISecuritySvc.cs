using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BioSCADA.WCFServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISecuritySvc" in both code and config file together.
    
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface ISecuritySvc
    {
        [OperationContract(IsInitiating = true)]
        bool Login(string name, string pass);

        [OperationContract(IsTerminating = true)]
        void Leave();
    }
}
