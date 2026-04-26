using GIS.City.Service.DTOs;
using GIS.City.Service.Entities;
using GIS.Common;

namespace GIS.City.Service
{
    public static class Extensions
    {
        public static CityInfoDTO AsCityDTO(this CityEntity entity)
        {
            return new CityInfoDTO(entity.Id, entity.CityName, entity.CityPopulation, entity.Districts != null ? entity.Districts.Select(district => district.DistrictAsDto()).ToList() : null);
        }

        public static DistrictBasicUpdateDTO AsDistrictBasicUpdateDTO(this DistrictDTO districtDTO)
        {
            return new DistrictBasicUpdateDTO(districtDTO.id, districtDTO.districtName, districtDTO.districtPopulation);
        } 

        public static DistrictDTO DistrictAsDto(this DistrictEntity districtEntity)
        {
            return new DistrictDTO(districtEntity.Id, districtEntity.Name, districtEntity.Population);
        }

        public static DistrictEntity DistrictAsEntity(this DistrictDTO districtDTO)
        {
            return new DistrictEntity()
            {
                Id = districtDTO.id,
                Name = districtDTO.districtName,
                Population = (long)districtDTO.districtPopulation
            };
        }

        public static BasicCityDTO AsCity(this CityEntity entity)
        {
            return new BasicCityDTO(entity.Id);
        }

        public static CityDTO InfoToDTO(this CityInfoDTO cityInfoDTO)
        {
            return new CityDTO(cityInfoDTO.id, cityInfoDTO.name, cityInfoDTO.population, cityInfoDTO.districts);
        }
    }
}
