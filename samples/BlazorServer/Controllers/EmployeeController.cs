using System.Threading.Tasks;
using DataTables.Blazored.Models;
using BlazorServer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorServer.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        [HttpPost("Data")]
        public async Task<IActionResult> Data([FromBody] TableRequestViewModel model)
        {
            var data = await _employeeRepository.GetDataAsync<object>(model, s => new
            {
                s.Id,
                s.FirstName,
                s.LastName,
                s.Office,
                s.Position,
                s.StartDate
            });
            return new JsonResult(data);
        }
    }
}
