using System;
using Microsoft.AspNetCore.Mvc;

namespace ExceptionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [HttpGet]
        [Route("weather/{min}")]
        public int GetWeather([FromRoute] int min)
        {
            var temp = new Random().Next(min, 50);
            return temp;
        }

        [HttpGet]
        [Route("weather")]
        public Weather GetWeather()
        {
            var temp = new Random().Next(-10, 50);
            return new Weather { Temperature = temp, Description = "XZ" };
        }

    }

    public class Weather
    {
        public int Temperature { get; set; }
        public string Description { get; set; }
    }
}