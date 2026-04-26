using GIS.Common;
using GIS.Country.Service.DTOs;
using GIS.Country.Service.Entities;

namespace GIS.Country.Service
{
    public static class Extensions
    {
        public static CountryDTO CountryAsDto(this CountryEntity countryEntity)
        {
            return new CountryDTO(countryEntity.Id, countryEntity.Name, countryEntity.Population, countryEntity.Cities != null ? countryEntity.Cities.Select(city => city.CityAsDto()).ToList() : null);
        }

        public static BasicCountryDTO AsCountry(this CountryEntity countryEntity)
        {
            return new BasicCountryDTO(countryEntity.Id);
        }

        public static DistrictDTO AsDistrictDTO(this DistrictEntity districtEntity)
        {
            return new DistrictDTO(districtEntity.Id, districtEntity.Name, districtEntity.Population);
        }

        public static DistrictEntity AsDistrictEntity(this DistrictDTO districtDTO)
        {
            return new DistrictEntity()
            {
                Id = districtDTO.id,
                Name = districtDTO.districtName,
                Population = (long)districtDTO.districtPopulation
            };
        }

        public static CityDTO CityAsDto(this CityEntity cityEntity) 
        {
            return new CityDTO(cityEntity.Id, cityEntity.Name, cityEntity.Population, cityEntity.Districts != null ? cityEntity.Districts.Select(district => district.AsDistrictDTO()).ToList() : null);
        }

        public static CityEntity CityAsEntity(this CityDTO cityDTO)
        {
            return new CityEntity()
            {
                Id = cityDTO.id, 
                Name = cityDTO.cityName, 
                Population = (long)cityDTO.cityPopulation,
                Districts = cityDTO.districts != null ? cityDTO.districts.Select(district => district.AsDistrictEntity()).ToList() : null
            };
        }
    }
}
