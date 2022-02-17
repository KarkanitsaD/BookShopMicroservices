using Microsoft.AspNetCore.Mvc;

namespace BookService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "TestFromBookService.";
        }
    }
}