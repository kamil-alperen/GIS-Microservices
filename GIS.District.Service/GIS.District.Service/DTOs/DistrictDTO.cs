using System.ComponentModel.DataAnnotations;

namespace GIS.City.Service.DTOs
{
    public record BasicDistrictDTO([Required] Guid id);
    public record DistrictCreateDTO([Required] Guid id, [Required] string districtName, [Required] Guid cityId, [Required] Guid countryId, long districtPopulation);
    public record DistrictBasicUpdateDTO([Required] Guid id, string? districtName, long? districtPopulation);
}
