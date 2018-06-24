using System;

namespace BioSCADA.ServerComponents
{
    public interface ITimer : IActiveObject
    {
        Action Tick { get; set; }
    }
}