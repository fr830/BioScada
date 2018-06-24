using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using Moq;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents
{
    public class when_testing_Interop : Specification
    {
        private Mock<IServer> server;
        private ServerInterop interop;
        private IEnumerable<Experiment> experimentsOnServer;
        private int userLevel = 20;
        [SetUp]
        public void Setup()
        {
            experimentsOnServer = new List<Experiment>() {
                new Experiment() { Name = "Exp1" },
                new Experiment() { Name = "Exp2" },
                new Experiment() { Name = "Exp3" },
            }.AsEnumerable();
            server = new Mock<IServer>(MockBehavior.Strict);
            server.
                Setup(x => x["Exp1"]).
                Returns(experimentsOnServer.ElementAt(0));
            interop = new ServerInterop(server.Object);
            interop.Login("GPR", "GPR");
            interop.User.UserLever = userLevel;
        }

        [Test]
        public void when_interop_start_server()
        {
            bool flag = false;
            server
                .Setup(x => x.Start(userLevel))
                .Callback(() => { flag = true; });
            interop.StartServer(userLevel);
            flag.ShouldBeTrue();
        }

        [Test]
        public void when_interop_stop_server()
        {
            bool flag = false;
            server
                .Setup(x => x.Stop(userLevel))
                .Callback(() => { flag = true; });
            interop.StopServer(userLevel);
            flag.ShouldBeTrue();
        }
        
        [Test]
        public void when_interop_start_expriment()
        {
            interop.StartExperiment("Exp1");
            experimentsOnServer.ElementAt(0).Started.ShouldBeTrue();
        }

        [Test]
        public void when_interop_stop_expriment()
        {
            interop.StartExperiment("Exp1");
            experimentsOnServer.ElementAt(0).Started.ShouldBeTrue();

            interop.StopExperiment("Exp1");
            experimentsOnServer.ElementAt(0).Started.ShouldBeFalse();
        }

        private Action<Dictionary<int, double>> callback;
        private Dictionary<int, double> varsReturn1;
        [Test]
        public void when_interop_GetVariables_expriment_and_return_the_alone_changedVariables()
        {
            varsReturn1 = new Dictionary<int, double>() { { 1, 20.0D }, { 2, 10 }, { 3, 5 } };
            callback = interop.CallbackAction;
            server
                .Setup(x => x.RequestVariables("Exp1", callback, userLevel))
                .Returns(true)
                .Callback(CallbackMockRequestVariables);

            interop.GetVariablesExperiment("Exp1").SequenceEqual(varsReturn1);
            interop.GetVariablesExperiment("Exp1").SequenceEqual(new Dictionary<int, double>());
        }

        private void CallbackMockRequestVariables()
        {
            callback.BeginInvoke(varsReturn1, null, null);
        }

        [Test]
        public void when_interop_set_value_variable()
        {
            server
                .Setup(x => x.WriteVariable("Exp1", 1, 30, userLevel))
                .Returns(true);
            interop.SetValueVariable(1, 30, "Exp1").ShouldBeTrue();
        }
    }
}
