using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoDataLoggerAPI.Models;
using MotoDataLoggerAPI.Repository;

namespace MotoDataLoggerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MotoDataController : ControllerBase
    {
        private readonly IMotoDataRepository _repository;
        public MotoDataController(IMotoDataRepository repository)
        {
            _repository = repository;
        }
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
        [HttpGet]
        public async Task<IActionResult> GetMotoData()
        {
            var motoDataList = await _repository.GetMotoDataAsync();
            return Ok(motoDataList);
        }

    }
}
