using GIS.City.Service.DTOs;
using GIS.City.Service.Entities;
using GIS.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GIS.City.Service.Services
{
    public class CityService : ICityService
    {
        private readonly HttpClient _httpClient;

        public CityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CountryExists(Guid countryId)
        {
            var response = await _httpClient.GetAsync($"Country/Get?id={countryId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCity(Guid countryId, Guid cityId)
        {
            var response = await _httpClient.DeleteAsync($"Country/DeleteCity?countryId={countryId}&cityId={cityId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCountry(Guid countryId, CityInfoDTO city)
        {
            CountryUpdateDTO countryUpdateDTO = new CountryUpdateDTO(
                countryId,
                null,
                null,
                city.InfoToDTO()
            );
            var response = await _httpClient.PutAsJsonAsync("Country", countryUpdateDTO);
            return response.IsSuccessStatusCode;
        }
    }
}
