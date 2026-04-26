using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS.Common
{
    public record CountryUpdateDTO([Required] Guid id, string? name, long? population, CityDTO? city);

    public record CityDTO([Required] Guid id, string? cityName, long? cityPopulation, List<DistrictDTO>? districts);

    public record CityUpdateDTO([Required] Guid id, string? name, long? population, DistrictDTO? district);

    public record DistrictDTO([Required] Guid id, string? districtName, long? districtPopulation);


}
