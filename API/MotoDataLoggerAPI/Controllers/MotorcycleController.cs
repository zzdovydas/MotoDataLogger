using Microsoft.AspNetCore.Mvc;
using MotoDataLoggerAPI.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace MotoDataLoggerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MotorcycleController : ControllerBase
    {
        private static List<Motorcycle> _motorcycles = new List<Motorcycle>();
        private static int _nextId = 1; // Simple ID generator

        [HttpGet]
        public IActionResult GetMotorcycles()
        {
            return Ok(_motorcycles);
        }

        [HttpGet("{id}")]
        public IActionResult GetMotorcycle(int id)
        {
            var motorcycle = _motorcycles.FirstOrDefault(m => m.Id == id);
            if (motorcycle == null)
            {
                return NotFound($"Motorcycle with ID {id} not found.");
            }
            return Ok(motorcycle);
        }

        [HttpPost]
        public IActionResult AddMotorcycle([FromBody] Motorcycle motorcycle)
        {
            if (motorcycle == null)
            {
                return BadRequest("Motorcycle data is null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            motorcycle.Id = _nextId++;
            _motorcycles.Add(motorcycle);

            return CreatedAtAction(nameof(GetMotorcycle), new { id = motorcycle.Id }, motorcycle);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMotorcycle(int id, [FromBody] Motorcycle updatedMotorcycle)
        {
            if (updatedMotorcycle == null)
            {
                return BadRequest("Motorcycle data is null.");
            }
            if(id != updatedMotorcycle.Id)
            {
                return BadRequest($"ID in the url ({id}) is different from id in the body ({updatedMotorcycle.Id})");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingMotorcycle = _motorcycles.FirstOrDefault(m => m.Id == id);
            if (existingMotorcycle == null)
            {
                return NotFound($"Motorcycle with ID {id} not found.");
            }

            existingMotorcycle.Manufacturer = updatedMotorcycle.Manufacturer;
            existingMotorcycle.Model = updatedMotorcycle.Model;
            existingMotorcycle.Year = updatedMotorcycle.Year;
            existingMotorcycle.Description = updatedMotorcycle.Description;
            existingMotorcycle.Vin = updatedMotorcycle.Vin;
            existingMotorcycle.LicensePlate = updatedMotorcycle.LicensePlate;
            existingMotorcycle.EngineDisplacement = updatedMotorcycle.EngineDisplacement;

            return Ok(existingMotorcycle);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMotorcycle(int id)
        {
            var motorcycle = _motorcycles.FirstOrDefault(m => m.Id == id);
            if (motorcycle == null)
            {
                return NotFound($"Motorcycle with ID {id} not found.");
            }

            _motorcycles.Remove(motorcycle);
            return NoContent();
        }
    }
}
