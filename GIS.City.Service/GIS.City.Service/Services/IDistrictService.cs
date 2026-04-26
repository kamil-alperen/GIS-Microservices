using GIS.City.Service.DTOs;

namespace GIS.City.Service.Services
{
    public interface IDistrictService
    {
        Task<bool> DeleteDistrict(Guid districtId);
        Task<bool> UpdateDistrict(Guid countryId, DistrictBasicUpdateDTO districtDTO);
    }
}
