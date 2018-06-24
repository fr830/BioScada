using System;
using System.Collections;

namespace BioSCADA.ServerComponents.DBLogger
{
    public static class LenghtSerializer
    {
        public static int GetBitsLengthInteger(int min, int max)
        {
            byte[] bytes = BitConverter.GetBytes((max - min));
            int length = 0;
            BitArray sd = new BitArray(bytes);
            for (int i = 0; i < sd.Length; i++)
            {
                if (sd[i])
                    length = i + 1;
            }
            return length;
        }

        public static int GetBitsLengthDouble(int min, int max, int lugaresDecimal)
        {
            int d = (int)(max * Math.Pow(10, lugaresDecimal)) - min;
            byte[] bytes = BitConverter.GetBytes(d);
            int length = 0;
            BitArray sd = new BitArray(bytes);
            for (int i = 0; i < sd.Length; i++)
            {
                if (sd[i])
                    length = i + 1;
            }
            return length;
        }

        public static int GetBytesLengthInteger(int min, int max)
        {
            int length = (int)Math.Ceiling(Math.Log(max - min, 2) / 8);
            return length;
        }

        public static int GetBytesLengthDouble(int min, int max, int lugaresDecimal)
        {
            int length = (int)Math.Ceiling(Math.Log((max * Math.Pow(10, lugaresDecimal)) - min, 2) / 8);
            return length;
        }
    }
}