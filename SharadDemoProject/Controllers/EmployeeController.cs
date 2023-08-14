using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharadDemoProject.Controllers.Context;
using SharadDemoProject.Model.Employees;
using System.Security.Claims;

namespace SharadDemoProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Hr,Manager")]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationContext _dbEmployee;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Logger<EmployeeController> _logger;

        public EmployeeController(ApplicationContext context, IHttpContextAccessor httpContextAccessor, Logger<EmployeeController> logger)
        {
            _dbEmployee = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeModel>>> GetEmployeeAsync()
        {
            var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            try
            {
                if (_dbEmployee.Employees == null)
                {
                    return NotFound();
                }
                List<EmployeeModel> employees = new List<EmployeeModel>();
                employees = null;
                employees.Add(employees[0]);
                return await _dbEmployee.Employees.ToListAsync();
            }
            catch (Exception ex)
            {

               _logger.LogError($"An error occurred: {ex.Message},login this user : {userName}");
                return StatusCode(500, new { Status = "Error", Message = "An error occurred while processing the request." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeModel>> GetEmployeeAsync(int id)
        {
            var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            try
            {
                if (_dbEmployee.Employees == null)
                {
                    return NotFound();
                }
                var employee = await _dbEmployee.Employees.FindAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }
                return employee;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"An error occurred: {ex.Message},login this user : {userName}");
                return StatusCode(500);
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<EmployeeModel>> PostEmployeeAsync(EmployeeModel employeeDetails)
        {
            var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            try
            {
                if (IsEmailAlreadyEntered(employeeDetails.EmpEmail))
                {
                    Serilog.Log.Warning($"Email ID: {employeeDetails.EmpEmail} already exists. login this user : {userName}");
                    return BadRequest("Email ID already exists.");
                }

                _dbEmployee.Employees.Add(employeeDetails);
                await _dbEmployee.SaveChangesAsync();

                return CreatedAtAction(null, null, new { id = employeeDetails.EmpId }, employeeDetails);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"An error occurred: {ex.Message},login this user : {userName}");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        private bool IsEmailAlreadyEntered(string email)
        {
            var existingEmployee = _dbEmployee.Employees.FirstOrDefault(e => e.EmpEmail.Equals(email));
            return existingEmployee != null;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeeAsync(int id, EmployeeModel employeeDetails)
        {
            try
            {
                if (id != employeeDetails.EmpId)
                {
                    return NotFound();
                }

                _dbEmployee.Entry(employeeDetails).State = EntityState.Modified;

                try
                {
                    await _dbEmployee.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeAvilable(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
                Serilog.Log.Error($"An error occurred: {ex.Message},login this user : {userName}");
                return StatusCode(500, $"An error occurred while processing the request.{ex.Message}");
            }
        }

        private bool EmployeeAvilable(int id)
        {
            return (_dbEmployee.Employees?.Any(x => x.EmpId == id)).GetValueOrDefault();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeAsync(int id)
        {
            try
            {
                if (_dbEmployee.Employees == null)
                {
                    return NotFound();
                }

                var employee = await _dbEmployee.Employees.FindAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }

                _dbEmployee.Employees.Remove(employee);

                await _dbEmployee.SaveChangesAsync();

                return Ok("Employee Deleted Successfully");
            }
            catch (Exception ex)
            {
                var userName = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
                Serilog.Log.Error($"An error occurred: {ex.Message},login this user : {userName}");
                return StatusCode(500, $"An error occurred while processing the request.{ex.Message}");
            }
        }
    }
}
