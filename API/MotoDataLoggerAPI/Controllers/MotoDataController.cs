using Microsoft.AspNetCore.Mvc;
using MotoDataLoggerAPI.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace MotoDataLoggerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MotoDataController : ControllerBase
    {
        private static List<MotoData> _motoDataList = new List<MotoData>();

        [HttpPost]
        public IActionResult PostMotoData([FromBody] MotoData data)
        {
            if (data == null)
            {
                return BadRequest("Data is null");
            }

            // Process the data here (e.g., save to database)
            Console.WriteLine(data.Timestamp);
            _motoDataList.Add(data);

            return Ok("Data received successfully");
        }

        [HttpGet]
        public IActionResult GetMotoData()
        {
            //returning all the data for now
            return Ok(_motoDataList);
        }
    }
}
