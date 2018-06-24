using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.DBLogger;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger
{
    public class when_testing_DBDeserializerTests : Specification
    {
        [Test]
        public void when_deserialize_variables()
        {
            string fileName = @"LogDay\Exp1_168344.bin";
            int id = 2;
            DBDeserializer.Deserialize(fileName, id)[0].ShouldEqual(0);
        }
    }
}
