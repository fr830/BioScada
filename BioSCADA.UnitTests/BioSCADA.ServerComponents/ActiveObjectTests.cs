using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents
{
    public class when_testing_active_objects : Specification
    {
        private ActiveObject obj;

        [SetUp]
        public void Setup()
        {
            obj = new ActiveObject();
        }

        [Test]
        public void then_can_start_experiment()
        {
            obj.Start();
            obj.Started.ShouldBeTrue();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Object already started")]
        public void then_fail_if_start_started_experiment()
        {
            obj.Start();
            obj.Start();

        }

        [Test]
        public void then_can_stop_experiment()
        {
            obj.Start();
            obj.Stop();
            obj.Started.ShouldBeFalse();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Object already stopped")]
        public void then_fail_if_stop_stopped_experiment()
        {
            obj.Stop();
            obj.Stop();
        }
    }
}
