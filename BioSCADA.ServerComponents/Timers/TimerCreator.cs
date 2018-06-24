using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Timers
{
    public class TimerCreator
    {
        public static RealtimeFixedIntervalTimer MCD(List<int> tick)
        {
            int result = int.MaxValue;
            for (int i = 0; i < tick.Count - 1; i++)
            {
                int euclidesMcd = EuclidesMCD(tick[i], tick[i + 1]);
                if (euclidesMcd < result)
                    result = euclidesMcd;
            }
            return new RealtimeFixedIntervalTimer() {Interval = result*250};
        }
        private static int EuclidesMCD(int a, int b)
        {
            int iaux;     //auxiliar
            a = Math.Abs(a);   //tomamos valor absoluto
            b = Math.Abs(b);
            int i1 = Math.Max(a, b);  //i1 = el más grande
            int i2 = Math.Min(a, b);  //i2 = el más pequeño
            do
            {
                iaux = i2;        //guardar divisor
                i2 = i1 % i2;       //resto pasa a divisor
                i1 = iaux;        //divisor pasa a dividendo
            } while (i2 != 0);
            return i1;            //ultimo resto no nulo
        }
    }
}
