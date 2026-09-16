using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;

namespace EnterpriseEmployeeManagement.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("chat")]
        [AllowAnonymous]
        public async Task<IActionResult> Chat([FromBody] AiChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return BadRequest(new { message = "Prompt cannot be empty." });
            }

            string query = request.Prompt.ToLower().Trim();
            string reply;

            try
            {
                // 1. Handle Employee specific queries (counting, listing, or searching specific names like "Ahmed Khan")
                if (query.Contains("employee") || query.Contains("staff") || query.Contains("worker") || query.Contains("who") || query.Contains("name"))
                {
                    int totalEmployees = await _context.Employees.CountAsync();

                    // Check if asking about a specific person's name
                    var allEmployees = await _context.Employees.ToListAsync();
                    var matchedEmployee = allEmployees.FirstOrDefault(e => query.Contains(e.FirstName.ToLower()) || query.Contains(e.LastName.ToLower()));

                    if (matchedEmployee != null)
                    {
                        reply = $"Yes! {matchedEmployee.FirstName} {matchedEmployee.LastName} is an active employee registered in the enterprise directory.";
                    }
                    else if (query.Contains("list") || query.Contains("show") || query.Contains("all") || query.Contains("names"))
                    {
                        var employeeList = allEmployees
                            .Take(10)
                            .Select(e => $"{e.FirstName} {e.LastName}")
                            .ToList();

                        if (employeeList.Any())
                        {
                            reply = $"There are {totalEmployees} total employees in the system. Here are some records: {string.Join(", ", employeeList)}.";
                        }
                        else
                        {
                            reply = "There are currently no employee records registered in the database.";
                        }
                    }
                    else
                    {
                        reply = $"There are currently {totalEmployees} active employees registered in the enterprise directory.";
                    }
                }
                // 2. Handle Department queries
                else if (query.Contains("department") || query.Contains("team") || query.Contains("branch"))
                {
                    int deptCount = await _context.Departments.CountAsync();
                    var deptNames = await _context.Departments
                        .Take(5)
                        .Select(d => d.Name)
                        .ToListAsync();

                    if (deptNames.Any())
                    {
                        reply = $"The enterprise manages {deptCount} departments, including: {string.Join(", ", deptNames)}.";
                    }
                    else
                    {
                        reply = $"The enterprise currently has {deptCount} registered departments.";
                    }
                }
                // 3. Handle Attendance queries
                else if (query.Contains("attendance") || query.Contains("check") || query.Contains("log") || query.Contains("presence"))
                {
                    int attendanceCount = await _context.Set<Domain.Entities.Attendance>().CountAsync();
                    reply = $"There are {attendanceCount} total recorded attendance logs currently stored in the database.";
                }
                // 4. Handle general greetings or conversational prompts
                else if (query.Contains("hi") || query.Contains("hello") || query.Contains("hey") || query.Contains("how are you"))
                {
                    reply = "Hello! I am your Enterprise AI Assistant. I am directly connected to your SQL database and ready to answer questions about your employees, departments, and attendance logs.";
                }
                // 5. Intelligent fallback summary for any other text
                else
                {
                    int totalEmp = await _context.Employees.CountAsync();
                    int totalDept = await _context.Departments.CountAsync();
                    int totalAtt = await _context.Set<Domain.Entities.Attendance>().CountAsync();

                    reply = $"I processed your query: \"{request.Prompt}\". System status overview: {totalEmp} employees, {totalDept} departments, and {totalAtt} attendance logs are currently synchronized and active in your database.";
                }
            }
            catch (Exception ex)
            {
                reply = $"Error querying enterprise database: {ex.Message}";
            }

            return Ok(new { reply });
        }
    }

    public class AiChatRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}