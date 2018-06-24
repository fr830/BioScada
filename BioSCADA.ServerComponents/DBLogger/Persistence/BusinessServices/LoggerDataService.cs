using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.DBLogger.Persistence.Repositories;

namespace BioSCADA.ServerComponents.DBLogger.Persistence.BusinessServices
{
    public interface ILoggerDataService : IBaseService<LoggerData, int>
    {
        
    }

    public class LoggerDataService : BaseService<LoggerData, int, ILoggerDataRepository>,ILoggerDataService
    {
        public LoggerDataService() : base(){}
        public LoggerDataService(ILoggerDataRepository repository) : base(repository) { }
    }
    
    public interface IUserService : IBaseService<User, Guid>
    {
        
    }

    public class UserService : BaseService<User, Guid, IUserRepository>, IUserService
    {
        public UserService() : base(){}
        public UserService(IUserRepository repository) : base(repository) { }
    }
}
