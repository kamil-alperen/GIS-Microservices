using GIS.City.Service.Entities;
using GIS.Common;
using System.ComponentModel.DataAnnotations;

namespace GIS.City.Service.DTOs
{
    public record BasicCityDTO([Required] Guid id);
    public record CityCreateDTO([Required] Guid id, [Required] string cityName, [Required] Guid countryId, long cityPopulation);
    public record CityInfoDTO([Required] Guid id, [Required] string name, long population, [Required] List<DistrictDTO> districts);
    public record DistrictBasicUpdateDTO([Required] Guid id, string? districtName, long? districtPopulation);
}
