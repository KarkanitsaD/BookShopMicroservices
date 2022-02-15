using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/hello")]
    public class HelloController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HelloController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Hello()
        {
            var httpClient = _httpClientFactory.CreateClient("helloApi");
            using var response = await httpClient.GetAsync("http://localhost:3000/api/hello");
            return Ok(await response.Content.ReadAsByteArrayAsync());
        }
    }
}