using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorServer.Data.Entities;
using BlazorServer.Interfaces;
using BlazorServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BlazorServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : DataController<Employee>
    {
        private ILogger _logger;
        private IHttpContextAccessor _httpContext;

        public EmployeesController(IHttpContextAccessor httpContext, ILoggerFactory loggerFactory, IGenericRepository<Employee> service) : base(service)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _httpContext = httpContext;
        }
        // GET: api/Animals
        /*[HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var collection = await Service.ReadAsync(s => s);
            return new JsonResult(collection);
        }*/

        // GET: api/Animals/5
        /*[HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }*/

        // POST: api/Animals
        /*[HttpPost]
        public void Post([FromBody] string value)
        {
        }*/

        // PUT: api/Animals/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromForm]AjaxViewModel form)
        {
            var d = _httpContext.HttpContext.Request;
            var selector = (new Employee()).Select(f => new
            {
                f.FirstName,
                f.LastName
            });
            await Task.Delay(1);
            return new JsonResult("1");
            //return await base.Data(model, selector);
        }
    }
}
