using GIS.City.Service.DTOs;
using GIS.City.Service.Entities;
using GIS.City.Service.Services;
using GIS.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Polly;

namespace GIS.City.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CityController : Controller
    {
        private readonly ILogger<CityController> _logger;
        private readonly IRepository<CityEntity> _repository;
        private readonly ICityService _cityService;
        private readonly IDistrictService _districtService;
        public CityController(ILogger<CityController> logger, IRepository<CityEntity> repository, ICityService cityService, IDistrictService districtService)
        {
            _logger = logger;
            _repository = repository;
            _cityService = cityService;
            _districtService = districtService;
        }

        [HttpGet("GetIds", Name = "GetIds")]
        public async Task<IActionResult> GetIdsAsync()
        {
            Result<IReadOnlyCollection<CityEntity>> result = await _repository.GetAllAsync();

            return result.IsSuccess ? Ok(result.Value?.Select(ent => ent.AsCity()).ToList()) : BadRequest(Results.Problem(
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
            Result<IReadOnlyCollection<CityEntity>> result = await _repository.GetAllAsync();

            return result.IsSuccess ? Ok(result.Value?.Select(ent => ent.AsCityDTO()).ToList()) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }

        [HttpGet("Get", Name = "Get")]
        public async Task<IActionResult> Get(Guid id)
        {
            Result<CityEntity> result = await _repository.GetAsync(id);

            return result.IsSuccess ? Ok(result.Value?.AsCityDTO()) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCity([FromBody] CityCreateDTO cityCreateDTO)
        {
            Result<CityEntity> getCity = await _repository.GetAsync(cityCreateDTO.id);

            if (getCity.IsSuccess)
            {
                return BadRequest(Results.Problem(
                        title: "Error occurred",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?>
                        {
                            { "errors", new[] { "City already exists" } }
                        }
                    ));
            }

            CityEntity cityEntity = new CityEntity()
            {
                Id = cityCreateDTO.id,
                CityName = cityCreateDTO.cityName,
                CityPopulation = cityCreateDTO.cityPopulation,
                CountryId = cityCreateDTO.countryId,
                Districts = new List<DistrictEntity>()
            };

            bool countryExists = await _cityService.CountryExists(cityCreateDTO.countryId);

            if (countryExists)
            {
                Result<CityEntity> result = await _repository.PostAsync(cityEntity);
                if (result.IsSuccess)
                {
                    CityEntity? createdEntity = result.Value;
                    bool countryUpdated = await _cityService.UpdateCountry(createdEntity.CountryId, createdEntity.AsCityDTO());
                    if (countryUpdated)
                    {
                        return Ok(createdEntity.AsCityDTO());
                    }

                    await DeleteCity(cityEntity.Id);
                    return BadRequest(Results.Problem(
                        title: "Error occurred",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?>
                        {
                            { "errors", new[] { "Country is not updated" } }
                        }
                    ));
                }
                return BadRequest(Results.Problem(
                        title: "Error occurred",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?>
                        {
                            { "errors", new[] { result.Error } }
                        }
                    ));
            }
            else
            {
                return BadRequest(Results.Problem(
                        title: "Error occurred",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?>
                        {
                            { "errors", new[] { "Country does not exist" } }
                        }
                    ));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCity([FromQuery] Guid countryId, [FromBody] CityUpdateDTO cityUpdateDTO)
        {
            Result<CityEntity> resultGet = await _repository.GetAsync(cityUpdateDTO.id);

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
            CityInfoDTO cityDTO = resultGet.Value?.AsCityDTO();

            if (cityDTO != null && cityUpdateDTO.district != null)
            {
                int index = cityDTO.districts.FindIndex(c => c.id == cityUpdateDTO.district.id);
                if (index != -1)
                {
                    cityDTO.districts[index] = cityDTO.districts[index] with
                    {
                        districtName = cityUpdateDTO.district.districtName,
                        districtPopulation = cityUpdateDTO.district.districtPopulation
                    };
                }
                else if (cityUpdateDTO.district.id != null & cityUpdateDTO.district.districtName != null & cityUpdateDTO.district.districtPopulation != null)
                {
                    cityDTO.districts.Add(cityUpdateDTO.district);
                }
            }

            CityEntity cityEntity = new CityEntity()
            {
                Id = cityUpdateDTO.id,
                CityName = cityUpdateDTO.name != null ? cityUpdateDTO.name : cityDTO.name,
                CountryId = countryId,
                CityPopulation = cityUpdateDTO.population != null ? (long)cityUpdateDTO.population : cityDTO.population,
                Districts = cityDTO?.districts.Select(district => district.DistrictAsEntity()).ToList<DistrictEntity>()
            };

            await _cityService.UpdateCountry(countryId, cityEntity.AsCityDTO());
            Result<bool> result = await _repository.PutAsync(cityEntity);

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
        public async Task<IActionResult> DeleteCity([FromQuery] Guid cityId)
        {
            Result<CityEntity> result = await _repository.GetAsync(entity => entity.Id == cityId);

            if (result.IsSuccess)
            {
                bool updateCountry = await _cityService.DeleteCity(result.Value.CountryId, result.Value.Id);

                if (updateCountry)
                {
                    await DeleteAllDistrictsOfCity(cityId);

                    CityEntity? cityEntity = result.Value;
                    Result<bool> deleteResult = await _repository.DeleteAsync(cityEntity.Id);
                    bool deleted = deleteResult.IsSuccess ? deleteResult.Value : false;

                    if (deleted)
                    {
                        return Ok();
                    }
                    else
                    {
                        await _cityService.UpdateCountry(result.Value.CountryId, result.Value.AsCityDTO());
                        return BadRequest(Results.Problem(
                                title: "Error occurred",
                                statusCode: StatusCodes.Status400BadRequest,
                                extensions: new Dictionary<string, object?>
                                {
                                    { "errors", new[] { deleteResult.Error } }
                                }
                            ));
                    }
                }
                else
                {
                    return BadRequest(Results.Problem(
                                title: "Error occurred",
                                statusCode: StatusCodes.Status400BadRequest,
                                extensions: new Dictionary<string, object?>
                                {
                                    { "errors", new[] { result.Error } }
                                }
                            ));
                }
            }
            else
            {
                return BadRequest(Results.Problem(
                            title: "Error occurred",
                            statusCode: StatusCodes.Status400BadRequest,
                            extensions: new Dictionary<string, object?>
                            {
                                { "errors", new[] { result.Error } }
                            }
                        ));
            }
        }

        private async Task<bool> DeleteAllDistrictsOfCity(Guid cityId)
        {
            Result<CityEntity> resultGet = await _repository.GetAsync(cityId);

            if (!resultGet.IsSuccess)
            {
                return false;
            }

            if (resultGet.Value.Districts.Count == 0)
            {
                return true;
            }

            foreach (var city in resultGet.Value.Districts)
            {
                bool response = await _districtService.DeleteDistrict(city.Id);
                if (!response) return false;
            }

            return true;
        }

        [HttpDelete("DeleteDistrict")]
        public async Task<IActionResult> DeleteDistrictFromCity([FromQuery] Guid cityId, [FromQuery] Guid districtId)
        {
            Result<CityEntity> resultGet = await _repository.GetAsync(cityId);

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

            DistrictEntity district = resultGet.Value.Districts.Find(district => district.Id == districtId);

            if (district == null)
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

            resultGet.Value.Districts.Remove(district);

            Result<bool> result = await _repository.PutAsync(resultGet.Value);

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
