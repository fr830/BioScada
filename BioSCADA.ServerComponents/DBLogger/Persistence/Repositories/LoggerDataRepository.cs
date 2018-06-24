using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace BioSCADA.ServerComponents.DBLogger.Persistence.Repositories
{
    public interface ILoggerDataRepository : IBaseRepository<LoggerData, int>
    {
        
    }

    public class LoggerDataRepository : BaseRepository<LoggerData, int>, ILoggerDataRepository
    {
        public LoggerDataRepository(ISession session)
            : base(session)
        {}

    }
}
