using System;
using NHibernate;

namespace BioSCADA.ServerComponents.DBLogger.Persistence.Repositories
{
    public class NHibernateBase
    {

        protected ISession session { get; set; }

        public NHibernateBase(ISession currentSession)
        {
            session = currentSession;
        }

        protected virtual TResult Transact<TResult>(
            Func<TResult> func)
        {
            if (!session.Transaction.IsActive)
            {
                // Wrap in transaction
                TResult result;
                using (var tx = session.BeginTransaction())
                {
                    result = func.Invoke();
                    tx.Commit();
                }
                return result;
            }
            // Don't wrap;
            return func.Invoke();
        }

        protected virtual void Transact(Action action)
        {
            Transact<bool>(() =>
                               {
                                   action.Invoke();
                                   return false;
                               });
        }

    }
}
