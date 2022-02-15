using System;
using System.Threading.Tasks;
using CustomerService.Data;
using CustomerService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerContext _context;

        public CustomersController(CustomerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Customers.ToListAsync());
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            return Ok(await _context.Customers.FirstOrDefaultAsync(c => c.Id == id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerModel createCustomer)
        {
            var entityToCreate = new CustomerEntity { Email = createCustomer.Email, Account = createCustomer.Account };
            var entity = await _context.Customers.AddAsync(entityToCreate);
            await _context.SaveChangesAsync();
            Console.WriteLine("Create customer: " + JsonConvert.SerializeObject(entityToCreate));

            //send message to rabbitMQ to some service

            return Ok(entity.Entity.Id);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCustomerModel updateCustomer)
        {
            var entityToUpdate = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == updateCustomer.Id);

            if (entityToUpdate == null)
            {
                return BadRequest("Customer not found.");
            }

            entityToUpdate.Email = updateCustomer.Email;

            _context.Update(entityToUpdate);
            await _context.SaveChangesAsync();
            Console.WriteLine("Update customer: " + JsonConvert.SerializeObject(entityToUpdate));

            return Ok(entityToUpdate);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var entityToDelete = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (entityToDelete == null)
            {
                return BadRequest("Customer not found.");
            }

            var entity = _context.Customers.Remove(entityToDelete);
            await _context.SaveChangesAsync();
            Console.WriteLine("Delete customer: " + JsonConvert.SerializeObject(entity.Entity));

            return Ok(entity.Entity);
        }
    }
}