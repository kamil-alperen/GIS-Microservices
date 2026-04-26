namespace GIS.Country.Service.Services
{
    public class CountryService : ICountryService
    {
        private readonly HttpClient _httpClient;

        public CountryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> DeleteCity(Guid cityId)
        {
            var response = await _httpClient.DeleteAsync($"City?cityId={cityId}");
            return response.IsSuccessStatusCode;
        }
    }
}
