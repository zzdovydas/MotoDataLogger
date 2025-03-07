using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotoDataLoggerAPI.Models;
using MotoDataLoggerAPI.Repository;

namespace MotoDataLoggerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MotorcycleController : ControllerBase
    {
        private readonly IMotorcycleRepository _repository;

        public MotorcycleController(IMotorcycleRepository repository)
         {
             _repository = repository;
         }

        [HttpGet]
        public async Task<IActionResult> GetMotorcycles()
        {
            var motorcycles = await _repository.GetMotorcyclesAsync();
            return Ok(motorcycles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMotorcycle(int id)
        {
            var motorcycle = await _repository.GetMotorcycleAsync(id);
            if (motorcycle == null)
            {
                return NotFound("Motorcycle not found");
            }
            return Ok(motorcycle);
        }

        [HttpPost]
        public async Task<IActionResult> AddMotorcycle([FromBody] Motorcycle motorcycle)
        {
            if (motorcycle == null)
            {
                return BadRequest("Motorcycle data is null");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdMotorcycle = await _repository.AddMotorcycleAsync(motorcycle);
            return CreatedAtAction(nameof(GetMotorcycle), new { id = createdMotorcycle.Id }, createdMotorcycle);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMotorcycle(int id, [FromBody] Motorcycle motorcycle)
        {
             if (motorcycle == null)
            {
                return BadRequest("Motorcycle is null");
            }
            if (id != motorcycle.Id)
            {
                return BadRequest("Id is incorrect");
            }
            var updatedMotorcycle = await _repository.UpdateMotorcycleAsync(id, motorcycle);
             if (updatedMotorcycle == null)
            {
                return NotFound("Motorcycle not found");
            }
            return Ok(updatedMotorcycle);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMotorcycle(int id)
        {
            var result = await _repository.DeleteMotorcycleAsync(id);
             if (!result)
            {
                return NotFound("Motorcycle not found");
            }
            return NoContent();
        }
    }
}
