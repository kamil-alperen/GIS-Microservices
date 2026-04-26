using GIS.City.Service.DTOs;
using GIS.City.Service.Entities;
using GIS.Common;

namespace GIS.City.Service
{
    public static class Extensions
    {
        public static DistrictDTO AsDistrictDTO(this DistrictEntity entity)
        {
            return new DistrictDTO(entity.Id, entity.DistrictName, entity.DistrictPopulation);
        }

        public static BasicDistrictDTO AsDistrict(this DistrictEntity entity)
        {
            return new BasicDistrictDTO(entity.Id);
        }
    }
}
