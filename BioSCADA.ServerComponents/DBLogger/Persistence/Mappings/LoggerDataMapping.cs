using FluentNHibernate.Mapping;

namespace BioSCADA.ServerComponents.DBLogger.Persistence.Mappings
{
    public class LoggerDataMapping : ClassMap<LoggerData>
    {
        public LoggerDataMapping()
        {
            Id(c => c.Id).GeneratedBy.Increment().Length(2);
            //Map(c => c.TimeStorage).Not.Nullable();
            Map(c => c.VariableId).Not.Nullable();
            Map(c => c.VariableValue).Not.Nullable();
        }
    }
}
