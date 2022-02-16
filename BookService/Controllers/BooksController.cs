using System;
using System.Data;
using System.Threading.Tasks;
using BookService.Data;
using BookService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookContext _context;

        public BooksController(BookContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Books.ToListAsync());
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            return Ok(await _context.Books.FirstOrDefaultAsync(b => b.Id == id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookModel model)
        {
            var entityToCreate = new BookEntity { Price = model.Price, Title = model.Title, Quantity = model.Quantity };
            var entity = await _context.Books.AddAsync(entityToCreate);
            await _context.SaveChangesAsync();

            return Ok(entity.Entity.Id);
        }

        [HttpPost]
        [Route("{id:int}/putAway/{count:int}")]
        public async Task<IActionResult> PutAwayBooks([FromRoute] int id, [FromRoute] int count)
        {
            if (count <= 0)
            {
                return BadRequest("Bad request.");
            }
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                var entity = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
                if (entity == null)
                {
                    return BadRequest("Book not found.");
                }

                if (count >= entity.Quantity)
                {
                    entity.Quantity = 0;
                }
                else
                {
                    entity.Quantity -= count;
                }

                _context.Books.Update(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return BadRequest();
            }
        }
    }
}