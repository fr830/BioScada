using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BioSCADA.ServerComponents;
using BioSCADA.ServerComponents.DBLogger;
using BioSCADA.ServerComponents.DBLogger.VariableSerializers;
using BioSCADA.ServerComponents.Drivers.DriversTest;
using BioSCADA.ServerComponents.Variables;
using Moq;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace BioSCADA.UnitTests.BioSCADA.ServerComponents.DBLogger
{
    public class when_testing_experiment_logger : Specification
    {
        [Test]
        public void then_can_save_variables()
        {
            Experiment exp = new Experiment();
            IDriver driver = new DriverInt() { ValueStorage = new ValueStorage() };

            IntegerVariable integerVariable = new IntegerVariable(0, "var1") { Driver = driver, MinValue = 0, MaxValue = 16, LogValues = true };
            exp.AddVariable(integerVariable); // 5 bits
            IntegerVariable integerVariable1 = new IntegerVariable(1, "var2") { Driver = driver, MinValue = 0, MaxValue = 1015 };
            exp.AddVariable(integerVariable1); // 10 bits
            BoolVariable boolVariable = new BoolVariable(2, "var3") { Driver = driver, LogValues = true };
            exp.AddVariable(boolVariable); // 1 bits
            driver.AddVariable(integerVariable);
            driver.AddVariable(integerVariable1);
            driver.AddVariable(boolVariable);
            ExperimentLoggerStub logger = new ExperimentLoggerStub(exp);
            Dictionary<int, double> varValues = new Dictionary<int, double>
                                                    {
                                                        {0, 23},
                                                        {1, 12},
                                                    };
            logger.ReceiveNewVariableValues(varValues);
            exp.Start(0);
            Thread.Sleep(50);
            logger.ExperimentSerializer.Serializers.Count.ShouldEqual(2);
            logger.ExperimentSerializer.Serializers.ElementAt(0).Value.ShouldBeInstanceOfType(typeof(IntegerVariableSerializer));
            logger.ExperimentSerializer.Serializers.ElementAt(1).Value.ShouldBeInstanceOfType(typeof(BoolVariableSerializer));

        }

        /*
         * Crear el fichero y salvar el encabezado
         * Crear los serializadores según el tipo de variables
         * Crear el bitstream en el fichero
         * Salvar los valores cada cierto tiempo
         * Vigilar al experimento si arranca o para
         * Recivir los valores que cambiaron y actualizar la copia local de estos valores ...
         
         */


        /*
         ExperimentSerializer
         * Dado un BitStream y un conjunto de variables (las que se salvan)
         * - Crear los serializadores segun el tipo de variables 
         * - Mantiene los valores de las variables
         */

        /*
         ExperimentLogger.ExperimentStarted()
         * Crea el FileStream con la hora
         * Selecciona las variables a salvar del experimento -> VarsToSave
         * Crea el encabezado y lo salva en el FileStream
         * Crea un BitStream utilizando el FileStream-----NO----
         * Crea un experimentSerializer con BitStream y VarsToSave
         * Calcula el tick del timer y crea el timer y .....
         * Cada tick del timer, llama a ExperimentSerializer.DoSerialize();
         */

        /*
         DBLogger
         * Recibe las notificaciones y las propaga
         * Crea los ExperimentLoggers en base a los experimentos existentes
         * Al final del dia:
         * - Detiene todos los ExpLoggers
         * - Zippea todos los ficheros y mueve el .zip para la carpeta de Salvas
         */
    }

    public class ExperimentLoggerStub : ExperimentLogger
    {
        private readonly Experiment _experiment;

        public ExperimentSerializer ExperimentSerializer
        {
            get { return _experimentSerializer; }
            set { _experimentSerializer = value; }
        }

        public ExperimentLoggerStub(Experiment experiment)
            : base(experiment)
        {
            _experiment = experiment;
        }

        public override void ReceiveNewVariableValues(Dictionary<int, double> variableValue)
        {
            ValuesReceive = variableValue;
        }

        public Dictionary<int, double> ValuesReceive { get; set; }
    }
}
