using System.Diagnostics.Metrics;
using System.Linq;
using GIS.Common;
using GIS.Common.Repositories;
using GIS.Country.Service.DTOs;
using GIS.Country.Service.Entities;
using GIS.Country.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GIS.Country.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CountryController : Controller
    {
        private readonly ILogger<CountryController> _logger;
        private readonly IRepository<CountryEntity> repository;
        private readonly ICountryService _countryService;

        public CountryController(ILogger<CountryController> logger, IRepository<CountryEntity> repository, ICountryService countryService)
        {
            this._logger = logger;
            this.repository = repository;
            this._countryService = countryService;
        }

        [HttpGet("GetIds", Name = "GetIds")]
        public async Task<IActionResult> GetIdsAsync()
        {
            Result<IReadOnlyCollection<CountryEntity>> result = await repository.GetAllAsync();

            return result.IsSuccess ? Ok(result.Value?.Select(ent => ent.AsCountry()).ToList()) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }

        [HttpGet("GetAll", Name = "GetAll")]
        public async Task<IActionResult> GetAllAsync()
        {
            Result<IReadOnlyCollection<CountryEntity>> result = await repository.GetAllAsync();

            return result.IsSuccess ? Ok(result.Value?.Select(ent => ent.CountryAsDto()).ToList()) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }

        [HttpGet("Get", Name = "Get")]
        public async Task<IActionResult> GetAsync([FromQuery] Guid id)
        {
            Result<CountryEntity> result = await repository.GetAsync(id);
            return result.IsSuccess ? Ok(result.Value?.CountryAsDto()) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCountry([FromBody] CountryCreateDTO countryCreateDTO)
        {
            Result<CountryEntity> resultGet = await repository.GetAsync(countryCreateDTO.id);

            if (resultGet.IsSuccess)
            {
                return BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { "Country already exists" } }
                    }
                ));
            }

            CountryEntity countryEntity = new CountryEntity()
            {
                Id = countryCreateDTO.id,
                Name = countryCreateDTO.name,
                Population = countryCreateDTO.population,
                Cities = new List<CityEntity>()
            };

            Result<CountryEntity> result = await repository.PostAsync(countryEntity);

            return result.IsSuccess ? CreatedAtRoute("Get", new { id = countryCreateDTO.id }, result.Value) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCountry([FromBody] CountryUpdateDTO countryUpdateDTO)
        {
            Result<CountryEntity> resultGet = await repository.GetAsync(countryUpdateDTO.id);

            if (!resultGet.IsSuccess)
            {
                return BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { resultGet.Error } }
                    }
                ));
            }
            CountryDTO countryDTO = resultGet.Value?.CountryAsDto();

            if (countryDTO != null && countryUpdateDTO.city != null)
            {
                int index = countryDTO.cities.FindIndex(c => c.id == countryUpdateDTO.city.id);
                if (index != -1)
                {
                    countryDTO.cities[index] = countryDTO.cities[index] with
                    {
                        cityName = countryUpdateDTO.city.cityName,
                        cityPopulation = countryUpdateDTO.city.cityPopulation,
                        districts = countryUpdateDTO.city.districts
                    };
                }
                else if (countryUpdateDTO.city.id != null & countryUpdateDTO.city.cityName != null & countryUpdateDTO.city.cityPopulation != null)
                {
                    countryDTO.cities.Add(countryUpdateDTO.city);
                }
            }

            CountryEntity countryEntity = new CountryEntity()
            {
                Id = countryUpdateDTO.id,
                Name = countryUpdateDTO.name != null ? countryUpdateDTO.name : countryDTO.name,
                Population = countryUpdateDTO.population != null ? (long)countryUpdateDTO.population : countryDTO.population,
                Cities = countryDTO.cities.Select(city => city.CityAsEntity()).ToList<CityEntity>()
            };

            Result<bool> result = await repository.PutAsync(countryEntity);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCountry([FromQuery] Guid id)
        {
            bool response = await DeleteAllCitiesOfCountry(id);
            if (!response)
            {
                return BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { "Cities of the country could not be deleted" } }
                    }
                ));
            }

            Result<bool> result = await repository.DeleteAsync(id);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }

        private async Task<bool> DeleteAllCitiesOfCountry(Guid countryId)
        {
            Result<CountryEntity> resultGet = await repository.GetAsync(countryId);

            if (!resultGet.IsSuccess)
            {
                return false;
            }

            if (resultGet.Value.Cities.Count == 0)
            {
                return true;
            }

            foreach (var city in resultGet.Value.Cities)
            {
                bool response = await _countryService.DeleteCity(city.Id);
                if (!response) return false;
            }

            return true;
        }

        [HttpDelete("DeleteCity")]
        public async Task<IActionResult> DeleteCityFromCountry([FromQuery] Guid countryId,  [FromQuery] Guid cityId)
        {
            Result<CountryEntity> resultGet = await repository.GetAsync(countryId);

            if (!resultGet.IsSuccess)
            {
                return BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { resultGet.Error } }
                    }
                ));
            }

            CityEntity city = resultGet.Value.Cities.Find(city => city.Id == cityId);

            if (city == null)
            {
                return BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { resultGet.Error } }
                    }
                ));
            }

            resultGet.Value.Cities.Remove(city);

            Result<bool> result = await repository.PutAsync(resultGet.Value);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }
    }
}
