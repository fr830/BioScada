using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioSCADA.ServerComponents.DBLogger;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger
{
    public class when_testing_LenghtSerializerTests : Specification
    {
        [Test]
        public void cuando_el_lenght_de_un_int_es_8bits()
        {
            int min = 0;
            int max = 255;
            LenghtSerializer.GetBitsLengthInteger(min, max).ShouldEqual(8);
        }

        [Test]
        public void cuando_el_min_es_0_y_max_es_16()
        {
            int min = 0;
            int max = 15;
            LenghtSerializer.GetBitsLengthInteger(min, max).ShouldEqual(4);
        }

        [Test]
        public void cuando_el_lenght_de_un_double_es_8_con_0_LugDec()
        {
            int min = 0;
            int max = 255;
            int lugaresDecimal = 0;
            LenghtSerializer.GetBitsLengthDouble(min, max, lugaresDecimal).ShouldEqual(8);
        }

        [Test]
        public void cuando_el_lenght_de_un_double_es_15_con_2_LugDec()
        {
            int min = 0;
            int max = 255;
            int lugaresDecimal = 2;
            LenghtSerializer.GetBitsLengthDouble(min, max, lugaresDecimal).ShouldEqual(15);
        }

        [Test]
        public void cuando_el_min_0_y_max_1_y_2_lugares_entonces_es_7()
        {
            int min = 0;
            int max = 1;
            int lugaresDecimal = 2;
            LenghtSerializer.GetBitsLengthDouble(min, max, lugaresDecimal).ShouldEqual(7);
        }

    }
}
