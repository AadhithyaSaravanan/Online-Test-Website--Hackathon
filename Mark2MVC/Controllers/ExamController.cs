using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Mark2MVC.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

namespace Mark2MVC.Controllers
{
    public class ExamController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;//dependency injection which is used to send request to api

        public ExamController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = Request.Cookies["userData"];//getting email from console

            // Make an API request to retrieve user details by email
            var client1 = _clientFactory.CreateClient("API");
            var response1 = await client1.GetAsync($"/api/Users/GetUserByEmail?email={userEmail}");//checking it

            if (response1.IsSuccessStatusCode)
            {
                var userData = await response1.Content.ReadAsAsync<User>(); // Read the content
                var userName = userData.FullName; //  the API response has a property called 'FullName'
                var welcomeMessage = $"Welcome, {userName}";
                ViewData["WelcomeMessage"] = welcomeMessage;
            }

            var tokenstring = Request.Cookies["jwt"];
            var client = _clientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
            var response = await client.GetAsync("/api/Courses");
            if (response.IsSuccessStatusCode)
            {
                var courses = await response.Content.ReadFromJsonAsync<IEnumerable<Course>>();
                return View(courses);
            }
            return View(null);
        }

        public async Task<IActionResult> Instruction()
        {
            var userEmail = Request.Cookies["userData"];
            var courseName = HttpContext.Request.Query["courseName"].ToString();

            try
            {
                var client = _clientFactory.CreateClient("API");//insted of writing localhost write this
                //check the completion
                var levelThreeCompletionResponse = await client.GetAsync($"/api/TestResult/CheckCompletion?email={userEmail}&courseName={courseName}&level=Level3");
                var levelThreeCompletion = JsonConvert.DeserializeObject<bool>(await levelThreeCompletionResponse.Content.ReadAsStringAsync());

                var levelTwoCompletionResponse = await client.GetAsync($"/api/TestResult/CheckCompletion?email={userEmail}&courseName={courseName}&level=Level2");
                var levelTwoCompletion = JsonConvert.DeserializeObject<bool>(await levelTwoCompletionResponse.Content.ReadAsStringAsync());

                var levelOneCompletionResponse = await client.GetAsync($"/api/TestResult/CheckCompletion?email={userEmail}&courseName={courseName}&level=Level1");
                var levelOneCompletion = JsonConvert.DeserializeObject<bool>(await levelOneCompletionResponse.Content.ReadAsStringAsync());

                if (levelThreeCompletion)
                {
                    TempData["WarningMessage"] = $"You have already completed the course {courseName} - Level3.";
                    return View("Message");
                }

                if (levelTwoCompletion)
                {
                    //get marks
                    var marksResponse = await client.GetAsync($"/api/TestResult/GetMarks?email={userEmail}&courseName={courseName}&level=Level2");

                    if (marksResponse.IsSuccessStatusCode)
                    {
                        var responseContent = await marksResponse.Content.ReadAsStringAsync();
                        var totalMarks = JsonConvert.DeserializeObject<int>(responseContent);

                        if (totalMarks > 1)
                        {
                            TempData["WelcomeMessage"] = "Welcome to Level 3!";
                            return RedirectToAction("LevelThree", "Exam", new { courseName });
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "You have failed to clear the exam on Level 2. You cannot procced further.";
                            ViewData["CourseName"] = courseName;
                            return View("Message");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to retrieve marks for Level 3. Please try again later.";
                        ViewData["CourseName"] = courseName;
                        return View("Message");
                    }
                }

                if (levelOneCompletion) 
                {
                    var marksResponse = await client.GetAsync($"/api/TestResult/GetMarks?email={userEmail}&courseName={courseName}&level=Level1");

                    if (marksResponse.IsSuccessStatusCode)
                    {
                        var responseContent = await marksResponse.Content.ReadAsStringAsync();
                        var totalMarks = JsonConvert.DeserializeObject<int>(responseContent);

                        if (totalMarks > 1)
                        {
                            TempData["WelcomeMessage"] = "Welcome to Level 2!";
                            return RedirectToAction("LevelTwo", "Exam", new { courseName });
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "You have failed to clear the exam on Level 1. You Cannot proceed further.";
                            ViewData["CourseName"] = courseName;
                            return View("Message");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to retrieve marks for Level 2. Please try again later.";
                        ViewData["CourseName"] = courseName;
                        return View("Message");
                    }
                }

                // Handle other conditions for Level 1, Level 2, etc.

                ViewData["CourseName"] = courseName;
                return View("Instruction"); // Replace "ErrorView" with the appropriate view name for error cases.
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as per your application's error handling strategy
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View("ErrorView"); // Replace "ErrorView" with the appropriate view name for error cases.
            }
        }
        public async Task<IActionResult> LevelOne(string courseName)
        {
            var tokenstring = Request.Cookies["jwt"];
            var httpClient = _clientFactory.CreateClient("API");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
           //getting actual value of placeholder from index
            var response = await httpClient.GetAsync($"/api/courses/{courseName}/levelonequestions"); // Updated URL

            if (response.IsSuccessStatusCode)
            {
                var questions = await response.Content.ReadAsAsync<List<Question>>();
                return View(questions);
            }
            else
            {
                return View("Error");
            }
        }


        public async Task<IActionResult> LevelTwo(string courseName)
        {
            var tokenstring = Request.Cookies["jwt"];
            var httpClient = _clientFactory.CreateClient("API");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
            var response = await httpClient.GetAsync($"/api/courses/{courseName}/leveltwoquestions");

            if (response.IsSuccessStatusCode)
            {
                var questions = await response.Content.ReadAsAsync<List<Question>>();
                return View(questions);
            }
            else
            {
                return View("Error");
            }
        }


        public async Task<IActionResult> LevelThree(string courseName)
        {
            var tokenstring = Request.Cookies["jwt"];
            var httpClient = _clientFactory.CreateClient("API");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
            var response = await httpClient.GetAsync($"/api/Courses/{courseName}/levelthreequestions");

            if (response.IsSuccessStatusCode)
            {
                var questions = await response.Content.ReadAsAsync<List<Question>>();
                return View(questions);
            }
            else
            {
                return View("Error");
            }
        }



        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> ResultPage(int totalMarks, string message, string courseName, string level)
        {
            var userEmail = Request.Cookies["userData"];

            // Make an API request to retrieve user details by email
            var client = _clientFactory.CreateClient("API");
            var response = await client.GetAsync($"/api/Users/GetUserByEmail?email={userEmail}");

            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(userJson);// Deserialization allows you to convert the JSON data into an object of the User class,

                // Map API response to TestResultDto
                var testResultReport = new TestResultReport
                {
                    Reg_Id = user.Reg_Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    State = user.State,
                    City = user.City,
                    CourseName = courseName,
                    TotalMarks = totalMarks,
                    Level = level,

                };

                // Convert DTO to JSON
                var jsonData = JsonConvert.SerializeObject(testResultReport);

                // Send a POST request to the API endpoint to store the data
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var postResponse = await client.PostAsync("/api/TestResult", content);

                if (postResponse.IsSuccessStatusCode)
                {
                    // Data stored successfully in the API, handle the success scenario here
                    TempData["SuccessMessage"] = "Data stored successfully!";
                }
                else
                {
                    // Handle API error responses here
                    var statusCode = (int)postResponse.StatusCode;
                    var errorMessage = await postResponse.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error occurred while storing data in API. Status Code: {statusCode}, Message: {errorMessage}";

                    // Redirect to an error page

                }

                // Data retrieval successful, display the user details
                ViewBag.UserDetails = testResultReport.FullName;
                ViewBag.TotalMarks = totalMarks;
                ViewBag.Message = message;
                ViewBag.CourseName = courseName;
                ViewBag.Level = level;

                // Redirect to the result page
                return View();
            }
            else
            {

                // Redirect to an error page
                return RedirectToAction("ErrorPage");
            }

        }
    }
}
