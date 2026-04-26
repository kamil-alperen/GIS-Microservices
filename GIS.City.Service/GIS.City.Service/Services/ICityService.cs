using GIS.City.Service.DTOs;
using GIS.City.Service.Entities;
using GIS.Common;
using Microsoft.AspNetCore.Mvc;

namespace GIS.City.Service.Services
{
    public interface ICityService
    {
        Task<bool> CountryExists(Guid countryId);
        Task<bool> UpdateCountry(Guid countryId, CityInfoDTO city);
        Task<bool> DeleteCity(Guid countryId, Guid cityId);
    }
}
