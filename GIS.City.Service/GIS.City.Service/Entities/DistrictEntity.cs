using GIS.Common;

namespace GIS.City.Service.Entities
{
    public class DistrictEntity : IEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public long Population { get; set; }
    }
}
