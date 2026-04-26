namespace GIS.Country.Service.Services
{
    public interface ICountryService
    {
        Task<bool> DeleteCity(Guid cityId);
    }
}
