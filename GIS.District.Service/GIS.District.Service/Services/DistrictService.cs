using GIS.City.Service.Entities;
using GIS.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GIS.City.Service.Services
{
    public class DistrictService : IDistrictService
    {
        private readonly HttpClient _httpClient;

        public DistrictService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CityExists(Guid cityId)
        {
            var response = await _httpClient.GetAsync($"City/Get?id={cityId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDistrict(Guid cityId, Guid districtId)
        {
            var response = await _httpClient.DeleteAsync($"City/DeleteDistrict?cityId={cityId}&districtId={districtId}");
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> UpdateCity(Guid countryId, Guid cityId, DistrictDTO district)
        {
            CityUpdateDTO cityUpdateDTO = new CityUpdateDTO(
                cityId,
                null,
                null,
                district
            );
            var response = await _httpClient.PutAsJsonAsync($"City?countryId={countryId}", cityUpdateDTO);
            return response.IsSuccessStatusCode;
        }
    }
}
