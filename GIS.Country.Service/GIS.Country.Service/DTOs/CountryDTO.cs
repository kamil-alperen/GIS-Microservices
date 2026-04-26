using GIS.Common;
using GIS.Country.Service.Entities;
using System.ComponentModel.DataAnnotations;

namespace GIS.Country.Service.DTOs
{
    public record BasicCountryDTO([Required] Guid id);
    public record CountryDTO([Required] Guid id, [Required] string name, long population, [Required] List<CityDTO> cities);
    public record CountryCreateDTO([Required] Guid id, [Required] string name, long population);
}