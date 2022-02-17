using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/hello")]
    public class HelloController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBookApi _booksApi;

        public HelloController(IHttpClientFactory httpClientFactory, IBookApi booksApi)
        {
            _httpClientFactory = httpClientFactory;
            _booksApi = booksApi;
        }

        [HttpGet]
        public async Task<IActionResult> Hello()
        {
            var httpClient = _httpClientFactory.CreateClient("helloApi");
            using var response = await httpClient.GetAsync("http://localhost:3000/api/hello");
            return Ok(await response.Content.ReadAsByteArrayAsync());
        }

        [HttpGet]
        [Route("getBooks")]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _booksApi.GetBooksAsync();

            return Ok(books.Content);
        }

        [HttpGet]
        [Route("getBooks/{id:int}")]
        public async Task<IActionResult> GetBook([FromRoute] int id)
        {
            var book = await _booksApi.GetBookAsync(id);

            return Ok(book);
        }

        [HttpPost]
        [Route("createBook")]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            await _booksApi.CreateBook(book);

            return Ok();
        }

        [HttpGet]
        [Route("exception")]
        public async Task GetException()
        {
            await _booksApi.GetException();
        }

    }


    public interface IBookApi
    {
        [Get("/api/books")]
        Task<ApiResponse<List<Book>>> GetBooksAsync();

        [Get("/api/books/{id}")]
        Task<Book> GetBookAsync([AliasAs("id")] int id);

        [Post("/api/books")]
        Task CreateBook([Body] Book book);

        [Post("/api/books/{id}/putAway/{count}")]
        Task PutAwayBook([AliasAs("id")] int id, [AliasAs("count")]int count);

        [Get("/api/books/exception")]
        Task GetException();
    }



    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}