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
            Console.WriteLine("Hello Get!");
            throw new Exception();
            
        }
    }
}