using System;
using System.Collections.Generic;
using System.Threading;
using BioSCADA.ServerComponents.DBLogger;
using BioSCADA.ServerComponents.DBLogger.Persistence.BusinessServices;
using BioSCADA.ServerComponents.DBLogger.Persistence.Repositories;
using System.Linq;

namespace BioSCADA.ServerComponents
{
    public class ServerInterop
    {
        private readonly IServer _server;
        public User User { get; private set; }
        UserService userService = new UserService(new UserRepository(DbHandlerSingleton.GetInstance().SessionSource.SessionFactory.GetCurrentSession()));

        public ServerInterop(IServer server)
        {
            _server = server;
        }

        public bool Login(string userName, string pass)
        {
            IList<User> users = userService.GetAll();

            IEnumerable<User> userCount = users.Where(x => x.Name == userName && x.Password == pass);
            if (userCount.Count() > 0)
                User = userCount.ElementAt(0);
            else
                User = null;
            
            if (User != null)
                return true;
            else
            {
                return false;
            }
        }

        public void StartServer(object startData = null)
        {
            if (User != null)
                _server.Start(startData);
        }

        public void StopServer(object startData = null)
        {
            if (User != null)
                _server.Stop(startData);
        }

        public void StartExperiment(string Name)
        {
            if (User != null)
                _server[Name].Start(User.UserLever);
        }

        public void StopExperiment(string Name)
        {
            if (User != null)
                _server[Name].Stop(User.UserLever);
        }

        Dictionary<int, double> VariablesCache = new Dictionary<int, double>();
        private Dictionary<int, double> changed;
        System.Threading.ManualResetEvent _resetEvent = new ManualResetEvent(true);
        public Dictionary<int, double> GetVariablesExperiment(string expName)
        {
            if (User != null)
            {
                _resetEvent.WaitOne();
                _resetEvent.Reset();
                try
                {
                    bool start = _server.RequestVariables(expName, CallbackAction, User.UserLever);

                    if (start)
                    {
                        _resetEvent.WaitOne();
                        _resetEvent.Set();
                    }
                    else
                        _resetEvent.Set();
                }
                catch
                {
                    _resetEvent.Set();
                }
                return changed;
            }
            else
            {
                return new Dictionary<int, double>();
            }
        }

        public void CallbackAction(Dictionary<int, double> obj)
        {
            changed = new Dictionary<int, double>();
            foreach (var d in obj)
            {
                if (VariablesCache.ContainsKey(d.Key))
                {
                    if (VariablesCache[d.Key] != d.Value)
                        changed.Add(d.Key, d.Value);
                }
                else
                    changed.Add(d.Key, d.Value);
                VariablesCache[d.Key] = d.Value;
            }
            //sal = true;
            _resetEvent.Set();
        }

        public bool SetValueVariable(int VariableId, double VariableValue, string ExperimentName)
        {
            if (User != null)
                return _server.WriteVariable(ExperimentName, VariableId, VariableValue, User.UserLever);
            else
            {
                return false;
            }
        }
    }
}