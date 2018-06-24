namespace BioSCADA.ServerComponents.DBLogger.VariableSerializers
{
    public interface IVariableSerializer
    {
        void Serialize(double newValue, IBitStream bitStream);
        double Deserialize(IBitStream bitStream);
        int BitLength { get; }
    }
}