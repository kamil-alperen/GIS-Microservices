using GIS.City.Service.DTOs;

namespace GIS.City.Service.Services
{
    public class DistrictService : IDistrictService
    {

        private readonly HttpClient _httpClient;

        public DistrictService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<bool> DeleteDistrict(Guid districtId)
        {
            var response = await _httpClient.DeleteAsync($"District?districtId={districtId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateDistrict(Guid countryId, DistrictBasicUpdateDTO districtDTO)
        {
            var response = await _httpClient.PutAsJsonAsync($"District?countryId={countryId}", districtDTO);
            return response.IsSuccessStatusCode;
        }
    }
}
