using System.Collections.Generic;
using BioSCADA.ServerComponents.DBLogger.Persistence.Repositories;
using GestContrat.Core.Ninject;
using Ninject;

namespace BioSCADA.ServerComponents.DBLogger.Persistence.BusinessServices
{
    public interface IBaseService<TData, TKey> where TData : class
    {
        TData Get(TKey id);
        IList<TData> GetAll();
        void Delete(TData data);
        void Update(TData data);
        TKey Save(TData data);
        void SaveOrUpdate(TData data);
    }

    public class BaseService<TData, TKey, TRepository> 
        where TData : class
        where TRepository : IBaseRepository<TData, TKey>
    {
        protected TRepository Repository;
        private readonly IKernel _kernel;

        public BaseService()
        {
            _kernel = new StandardKernel(new CoreModule());
            Repository = _kernel.Get<TRepository>();
        }

        public BaseService (TRepository repository)
        {
            Repository = repository;
        }

        public TData Get(TKey id)
        {
            return Repository.Get(id);
        }

        public IList<TData> GetAll()
        {
            return Repository.GetAll();
        }

        public void Delete(TData data)
        {
            Repository.Delete(data);
        }

        public void Update(TData data)
        {
            Repository.Update(data);
        }

        public TKey Save(TData data)
        {
            return Repository.Save(data);
        }

        public void SaveOrUpdate(TData data)
        {
            Repository.SaveOrUpdate(data);
        }
    }
}
