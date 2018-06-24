using System;
using System.Collections.Generic;
using System.IO;
using BioSCADA.ServerComponents.Variables;

namespace BioSCADA.ServerComponents.DBLogger
{
    public static class DBDeserializer
    {
        public static List<double> Deserialize(string fileName, int id)
        {
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);

            BinaryReader reader = new BinaryReader(fileStream);
            int count = (int)reader.ReadInt64();
            byte[] buffer = new byte[count];
            fileStream.Read(buffer, 0, count);
            MemoryStream varHeaderStream = new MemoryStream(buffer);
            varHeaderStream.Seek(0, SeekOrigin.Begin);
            var xxx = (List<Variable>)BinarySerializer.Deserialize(varHeaderStream);
            ExperimentSerializer serializer = new ExperimentSerializer(xxx);
            Dictionary<int, List<double>> values = new Dictionary<int, List<double>>();
            foreach (var variable in xxx)
            {
                values.Add(variable.ID, new List<double>());
            }

            MemoryStream memoryStr = new MemoryStream();
            fileStream.CopyTo(memoryStr);

            BitStream stream = new BitStream(memoryStr);
            stream.Position = 0;
            while (stream.Position < stream.Length)
            {
                var aux = serializer.Deserialize(stream);
                foreach (var d in aux)
                    values[d.Key].Add(d.Value);
            }
            if (values.ContainsKey(id))
                return values[id];
            else
            {
                throw new Exception(string.Format("El id {0} not found", id));
            }
        }

        public static Dictionary<int, Tuple<DateTime, double>> Deserialize(List<int> vars, DateTime from, DateTime to)
        {
            //var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);

            //BinaryReader reader = new BinaryReader(fileStream);
            //int count = (int)reader.ReadInt64();
            //byte[] buffer = new byte[count];
            //fileStream.Read(buffer, 0, count);
            //MemoryStream varHeaderStream = new MemoryStream(buffer);
            //varHeaderStream.Seek(0, SeekOrigin.Begin);
            //var xxx = (List<Variable>)BinarySerializer.Deserialize(varHeaderStream);
            //ExperimentSerializer serializer = new ExperimentSerializer(xxx);
            //Dictionary<int, List<double>> values = new Dictionary<int, List<double>>();
            //foreach (var variable in xxx)
            //{
            //    values.Add(variable.ID, new List<double>());
            //}

            //MemoryStream memoryStr = new MemoryStream();
            //fileStream.CopyTo(memoryStr);

            //BitStream stream = new BitStream(memoryStr);
            //stream.Position = 0;
            //while (stream.Position < stream.Length)
            //{
            //    var aux = serializer.Deserialize(stream);
            //    foreach (var d in aux)
            //        values[d.Key].Add(d.Value);
            //}
            //return values[id];
            return new Dictionary<int, Tuple<DateTime, double>>();
        }
    }
}