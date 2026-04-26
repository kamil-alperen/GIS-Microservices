using GIS.City.Service.Entities;
using GIS.Common;
using Microsoft.AspNetCore.Mvc;

namespace GIS.City.Service.Services
{
    public interface IDistrictService
    {
        Task<bool> CityExists(Guid cityId);
        Task<bool> UpdateCity(Guid countryId, Guid cityId, DistrictDTO district);
        Task<bool> DeleteDistrict(Guid cityId, Guid districtId);
    }
}
