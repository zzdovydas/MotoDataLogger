using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoDataLoggerAPI.Models;
using MotoDataLoggerAPI.Repository;

namespace MotoDataLoggerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MotoDataController : ControllerBase
    {
        private readonly IMotoDataRepository _repository;
        public MotoDataController(IMotoDataRepository repository)
        {
            _repository = repository;
        }

        [Authorize]
        [HttpPost("PostMotoDataByAPIKey")]
        public async Task<IActionResult> PostMotoDataByAPIKey([FromBody] MotoData motoData)
        {
            if (motoData == null)
            {
                return BadRequest("Data is null");
            }

            await _repository.AddMotoDataByAPIKeyAsync(motoData);
            return Ok("Data received successfully");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostMotoData([FromBody] MotoData motoData)
        {
            if (motoData == null)
            {
                return BadRequest("Data is null");
            }

            await _repository.AddMotoDataAsync(motoData);
            return Ok("Data received successfully");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMotoData()
        {
            var motoDataList = await _repository.GetMotoDataAsync();
            return Ok(motoDataList);
        }

    }
}
