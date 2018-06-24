using System.Collections.Generic;
using NHibernate;

namespace BioSCADA.ServerComponents.DBLogger.Persistence.Repositories
{

    public interface IBaseRepository<TData, TKey> where TData : class 
    {
        TData Get(TKey id);
        IList<TData> GetAll();
        TKey Save(TData data);
        void Update(TData data);
        void Delete(TData data);
        void SaveOrUpdate(TData data);
    }
    
    public class BaseRepository<TData,TKey> : NHibernateBase
        where TData : class
    {
        public BaseRepository(ISession session) : base(session)
        {
        }

        public TKey Save(TData data)
        {
            return Transact(() => (TKey)session.Save(data));
        }

        public void SaveOrUpdate(TData data)
        {
            Transact(() => session.SaveOrUpdate(data));
        }

        public TData Get(TKey id)
        {
            return Transact(() => session.Get<TData>(id));
        }

        public IList<TData> GetAll()
        {
            return Transact(() => session.QueryOver<TData>().List());
        }

        public void Delete(TData data)
        {
            Transact(() => session.Delete(data));
        }

        public void Update(TData data)
        {
            Transact(() => session.Update(data));
        }

    }

}
