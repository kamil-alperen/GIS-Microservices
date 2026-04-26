using GIS.City.Service.DTOs;
using GIS.City.Service.Entities;
using GIS.Common;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using GIS.City.Service.Services;

namespace GIS.City.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DistrictController : Controller
    {
        private readonly ILogger<DistrictController> _logger;
        private readonly IRepository<DistrictEntity> _repository;
        private readonly IDistrictService _districtService;
        public DistrictController(ILogger<DistrictController> logger, IRepository<DistrictEntity> repository, IDistrictService districtService) {
            _logger = logger;
            _repository = repository;
            _districtService = districtService;
        }

        [HttpGet("GetIds", Name = "GetIds")]
        public async Task<IActionResult> GetIdsAsync()
        {
            Result<IReadOnlyCollection<DistrictEntity>> result = await _repository.GetAllAsync();

            return result.IsSuccess ? Ok(result.Value?.Select(ent => ent.AsDistrict()).ToList()) : BadRequest(Results.Problem(
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
            Result<IReadOnlyCollection<DistrictEntity>> result = await _repository.GetAllAsync();

            return result.IsSuccess ? Ok(result.Value?.Select(ent => ent.AsDistrictDTO()).ToList()) : BadRequest(Results.Problem(
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
            Result<DistrictEntity> result = await _repository.GetAsync(id);

            return result.IsSuccess ? Ok(result.Value?.AsDistrictDTO()) : BadRequest(Results.Problem(
                    title: "Error occurred",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>
                    {
                        { "errors", new[] { result.Error } }
                    }
                ));
        }

        [HttpPost]
        public async Task<IActionResult> CreateDistrict([FromBody] DistrictCreateDTO districtCreateDTO)
        {
            Result<DistrictEntity> getDistrict = await _repository.GetAsync(districtCreateDTO.id);

            if (getDistrict.IsSuccess)
            {
                return BadRequest(Results.Problem(
                        title: "Error occurred",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?>
                        {
                            { "errors", new[] { "District already exists" } }
                        }
                    ));
            }

            DistrictEntity districtEntity = new DistrictEntity()
            {
                Id = districtCreateDTO.id,
                DistrictName = districtCreateDTO.districtName,
                DistrictPopulation = districtCreateDTO.districtPopulation,
                CityId = districtCreateDTO.cityId
            };

            bool cityExists = await _districtService.CityExists(districtCreateDTO.cityId);

            if (cityExists)
            {
                Result<DistrictEntity> result = await _repository.PostAsync(districtEntity);
                if (result.IsSuccess)
                {
                    DistrictEntity? createdEntity = result.Value;
                    bool countryUpdated = await _districtService.UpdateCity(districtCreateDTO.countryId, createdEntity.CityId, createdEntity.AsDistrictDTO());
                    if (countryUpdated)
                    {
                        return Ok(createdEntity.AsDistrictDTO());
                    }

                    await DeleteDistrict(districtCreateDTO.countryId, districtEntity.Id);
                    return BadRequest(Results.Problem(
                        title: "Error occurred",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?>
                        {
                            { "errors", new[] { "City is not updated" } }
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
                            { "errors", new[] { "City does not exist" } }
                        }
                    ));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDistrict([FromQuery] Guid countryId, [FromBody] DistrictBasicUpdateDTO districtUpdateDTO)
        {
            Result<DistrictEntity> previousDistrict = await _repository.GetAsync(districtUpdateDTO.id);

            if (previousDistrict.IsSuccess)
            {
                DistrictEntity districtEntity = new DistrictEntity()
                {
                    Id = districtUpdateDTO.id,
                    DistrictName = districtUpdateDTO.districtName != null ? districtUpdateDTO.districtName : previousDistrict.Value.DistrictName,
                    CityId = previousDistrict.Value.CityId,
                    DistrictPopulation = districtUpdateDTO.districtPopulation != null ? (long)districtUpdateDTO.districtPopulation : 0,
                };

                Result<bool> updateResult = await _repository.PutAsync(districtEntity);

                if (updateResult.IsSuccess)
                {
                    bool cityUpdated = await _districtService.UpdateCity(countryId, previousDistrict.Value.CityId, districtEntity.AsDistrictDTO());
                    if (cityUpdated)
                    {
                        return Ok(updateResult.Value);
                    }
                    else
                    {
                        await _repository.PutAsync(previousDistrict.Value);
                        return BadRequest(Results.Problem(
                            title: "Error occurred",
                            statusCode: StatusCodes.Status400BadRequest,
                            extensions: new Dictionary<string, object?>
                            {
                                { "errorsUpdateDistrict", new[] { updateResult.Error } }
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
                                { "errorsUpdateDistrict", new[] { updateResult.Error } }
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
                                { "errorsPreviousDistrict", new[] { previousDistrict.Error } }
                        }
                    ));
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDistrict([FromQuery] Guid countryId, [FromQuery] Guid districtId)
        {
            Result<DistrictEntity> result = await _repository.GetAsync(entity => entity.Id == districtId);

            if (result.IsSuccess)
            {
                bool updateCity = await _districtService.DeleteDistrict(result.Value.CityId, result.Value.Id);

                if (updateCity)
                {
                    DistrictEntity? districtEntity = result.Value;
                    Result<bool> deleteResult = await _repository.DeleteAsync(districtEntity.Id);
                    bool deleted = deleteResult.IsSuccess ? deleteResult.Value : false;

                    if (deleted)
                    {
                        return Ok();
                    }
                    else
                    {
                        await _districtService.UpdateCity(countryId, result.Value.CityId, result.Value.AsDistrictDTO());
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


    }
}
