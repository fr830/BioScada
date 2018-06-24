using System;
using NHibernate;

namespace BioSCADA.ServerComponents.DBLogger.Persistence.Repositories
{

    public interface IUserRepository : IBaseRepository<User, Guid>
    {

    }

    public class UserRepository : BaseRepository<User, Guid>, IUserRepository
    {
        public UserRepository(ISession session)
            : base(session)
        {}

    }
}