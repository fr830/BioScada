using FluentNHibernate.Mapping;

namespace BioSCADA.ServerComponents.DBLogger.Persistence.Mappings
{
    public class UserMapping : ClassMap<User>
    {
        public UserMapping()
        {
            Id(c => c.Id).GeneratedBy.Guid();
            Map(c => c.Name).Not.Nullable();
            Map(c => c.Password).Not.Nullable();
            Map(c => c.UserLever).Not.Nullable();
        }
    }
}