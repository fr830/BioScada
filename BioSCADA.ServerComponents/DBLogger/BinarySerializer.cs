using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BioSCADA.ServerComponents.DBLogger
{
    public class BinarySerializer
    {
        public static void Serialize(object obj, string FileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(FileName, FileMode.Create);
            formatter.Serialize(stream, obj);
            stream.Close();
        }
        public static void Serialize(object obj, Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
        }
        public static object Deserialize(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }
        public static object Clone (object Source)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(Source, stream);
            stream.Seek(0, SeekOrigin.Begin);
            return Deserialize(stream);
        }
        public static object Deserialize(string FileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(FileName, FileMode.Open);
            object Result = formatter.Deserialize(stream);
            stream.Close();
            return Result;
        }
    }
}
