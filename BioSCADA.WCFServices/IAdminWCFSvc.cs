using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BioSCADA.WCFServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAdminWCFSvc" in both code and config file together.
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IAdminWCFSvc : ISecuritySvc
    {
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        bool Start_Server();
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        bool Stop_Server();
    }
}
