using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioSCADA.ServerComponents.Alarms
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now { get { return DateTime.Now; } }
    }
}
