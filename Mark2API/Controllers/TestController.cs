using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mark2API.Models; // Assuming your models are in this namespace
using System.Linq;

namespace Mark2API.Controllers
{
    [Route("api/[controller]")]//route endpoint
    [ApiController]
    public class TestResultController : Controller//define classname and inherit from it
    {
        private readonly OnlineDbContext _dbContext; // dbcontext injection

        public TestResultController(OnlineDbContext dbContext)//creating a instance for online and assign to _dbcontext
        {
            _dbContext = dbContext;
        }

        // Action method to handle test result submission

        [HttpPost]
        public IActionResult PostTestResult([FromBody] TestResultReport testResultReport)//to recive the data check
        {
            if (testResultReport != null)
            {
                try
                {
                    // Store the testResultReport object in the database
                    _dbContext.Results.Add(testResultReport);
                    _dbContext.SaveChanges();//save it

                    // Return a success response
                    return Ok("Data stored successfully!");
                }
                catch (Exception ex)
                {
                    // Handle database or other errors
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            // If the testResultReport object is null, return a bad request response
            return BadRequest("Invalid data received.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestResultReport>>> GetCourses()//get all the test report actin method change because to http request
        {
            if (_dbContext.Results == null)
            {
                return NotFound();
            }
            return await _dbContext.Results.ToListAsync();
        }

        [HttpGet("GetByEmail")]
        public async Task<ActionResult<IEnumerable<TestResultReport>>> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email parameter is required.");
            }

            var user = await _dbContext.Results.Where(r => r.Email == email).ToListAsync();//check results of email


            if (user == null)
            {
                return NotFound($"No records found for email: {email}");
            }

            // If the email is found, return the data for that email
            return Ok(user);
        }

        [HttpGet]
        [Route("CheckCompletion")]
        public async Task<IActionResult> CheckCompletion(string email, string courseName, string level)
        {
            try
            {
                // Query the database or data source to check if the user has completed the given level
                var result = await _dbContext.Results
                    .AnyAsync(r => r.Email == email && r.CourseName == courseName && r.Level == level);

                return Ok(result); // Return true if completed, false if not completed
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetMarks")]//checking whether they are allowed to move next
        public async Task<IActionResult> GetMarks(string email, string courseName, string level)
        {
            try
            {
                // Retrieve marks from the database based on email, courseName, and level
                var result = await _dbContext.Results
                    .FirstOrDefaultAsync(r => r.Email == email && r.CourseName == courseName && r.Level == level);

                if (result != null)
                {
                    int? marks = result.TotalMarks;//storing even it is even nullable
                    return Ok(marks); // Return marks as JSON response
                }
                else
                {
                    // Marks not found, return appropriate response
                    return NotFound("Marks not found for the specified user, course, and level.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as per your application's error handling strategy
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
        [HttpGet("Search")]
        public async Task<IActionResult> Search(//try to pass those in url to get specified
                 [FromQuery] string? courseName,
                 [FromQuery] string? state,
                 [FromQuery] string? city,
                 [FromQuery] string? level,
                 [FromQuery] int? marks)
        {
            IQueryable<TestResultReport> query = _dbContext.Results;//creating a query inorder to use it later

            // Check if at least one search parameter is provided

            if (string.IsNullOrEmpty(courseName) && string.IsNullOrEmpty(state) && string.IsNullOrEmpty(city) && string.IsNullOrEmpty(level) && !marks.HasValue)
            {
                // At least one search parameter is required
                return BadRequest("At least one search parameter is required.");
            }

            if (!string.IsNullOrEmpty(courseName))
            {
                query = query.Where(r => r.CourseName == courseName);//checking the query to filter
            }

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(r => r.State == state);
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(r => r.City == city);
            }

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(r => r.Level == level);
            }

            if (marks.HasValue)
            {
                query = query.Where(r => r.TotalMarks >= marks);
            }

            var searchResults = await query.ToListAsync();

            if (searchResults.Count == 0)
            {
                return NotFound("No records found matching the search criteria.");
            }

            return Ok(searchResults);
        }
       

    }
}

