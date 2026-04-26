using System.ComponentModel.DataAnnotations;
using GIS.Common;

namespace GIS.City.Service.Entities
{
    public class DistrictEntity : IEntity
    {
        public Guid Id { get; set; }
        public string DistrictName { get; set; }
        public Guid CityId { get; set; }
        public long DistrictPopulation { get; set; }
    }
}
