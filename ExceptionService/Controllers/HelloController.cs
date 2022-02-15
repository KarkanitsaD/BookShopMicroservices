using System;
using Microsoft.AspNetCore.Mvc;

namespace ExceptionService.Controllers
{
    [ApiController]
    [Route("api/hello")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult Hello()
        {
            var i = new Random().Next(1, 10);
            if (i < 5)
            {
                HttpContext.Response.StatusCode = 418;
                throw new Exception();
            }
            else
            {
                return Ok("Hello World!");
            }
        }
    }
}