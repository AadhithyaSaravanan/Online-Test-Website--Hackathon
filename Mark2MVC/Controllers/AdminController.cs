using DNTCaptcha.Core;
using Mark2MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Mark2MVC.Models.Question;

namespace Mark2MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;//dependency injection which is used to send request to external service

        public AdminController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Fetch()
        {
            var tokenstring = Request.Cookies["jwt"];//requesting the token
            var client = _clientFactory.CreateClient("API");//using client name
            //token value from bearer because of long request from headers "bearer is type"
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
            var response = await client.GetAsync("/api/Questions");
            if (response.IsSuccessStatusCode)
            {
                var questions = await response.Content.ReadFromJsonAsync<IEnumerable<Question>>();
                return View(questions);
            }
            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel(IFormFile uploadedFile)
        {
            try
            {
                // Check if a file is provided and has content
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    // Create a memory stream to read the file content
                    using (var memoryStream = new MemoryStream())
                    {
                        // Copy the file content to the memory stream
                        await uploadedFile.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        //send as multipart form data 
                        var multipartContent = new MultipartFormDataContent
                        {
                            { new StreamContent(memoryStream), "fileinput", uploadedFile.FileName }//representing as binary content with file name
                        };
                        var tokenstring = Request.Cookies["jwt"];
                        var apiClient = _clientFactory.CreateClient("API");
                        var apiUrl = "/api/Questions/upload-file"; // API endpoint URL
                        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
                        // Make a POST request to the API endpoint with the file content

                        var response = await apiClient.PostAsync(apiUrl, multipartContent);

                        if (!response.IsSuccessStatusCode)
                        {
                            TempData["UploadErrors"] = "An error occurred while uploading the file. Please try again later.";
                        }
                        else
                        {
                            TempData["UploadMessage"] = "File Uploaded Successfully";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                ViewBag.ErrorMessage = "An unexpected error occurred while processing your request. Please try again later.";
                Console.WriteLine(ex.ToString());
            }

            return RedirectToAction("AddQuestions");
        }


        [HttpGet]
        public IActionResult AddQuestions()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestions(Question question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var tokenstring = Request.Cookies["jwt"];
                    var client = _clientFactory.CreateClient("API");
                    var content = new StringContent(JsonConvert.SerializeObject(question), Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);

                    HttpResponseMessage response = await client.PostAsync("/api/Questions", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["CreateMessage"] = "Question Added Successfully";
                        return RedirectToAction("AddQuestions");
                    }
                    else
                    {
                        // For better debugging and user feedback, you might want to extract the response error message
                        TempData["ErrorMessage"] = "Invalid CourseId. Please Enter a valid CourseId.";
                        return RedirectToAction("AddQuestions");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                ViewBag.ErrorMessage = "An unexpected error occurred while processing your request. Please try again later.";
                Console.WriteLine(ex.ToString());
            }

            return View();
        }
        
        [HttpGet]
        public IActionResult RemoveQuestions()
        {
            
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                var tokenstring = Request.Cookies["jwt"];
                var apiClient = _clientFactory.CreateClient("API");//http client creation
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
                var response = await apiClient.DeleteAsync($"/api/Questions/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"Question {id} deleted successfully. Status Code: {response.StatusCode}";
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = $"Question {id} not found. Status Code: {response.StatusCode}";
                }
                else
                {
                    TempData["Error"] = $"Failed to delete question {id}. Status Code: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An unexpected error occurred: {ex.Message}";
            }

            return RedirectToAction("RemoveQuestions"); // Redirect to the appropriate view
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAllQuestionsConfirmed()
        {
            try
            {
                var tokenstring = Request.Cookies["jwt"];
                var apiClient = _clientFactory.CreateClient("API");
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
                var response = await apiClient.DeleteAsync("/api/Questions/all");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "All questions deleted successfully.";
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "Questions not found. Status Code: 404";
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    TempData["Error"] = "Bad request. Please check your input. Status Code: 400";
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["Error"] = "Unauthorized access. Status Code: 401";
                }
                else
                {
                    TempData["Error"] = $"Failed to delete questions. Status Code: {(int)response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An unexpected error occurred: {ex.Message}";
            }

            return RedirectToAction("RemoveQuestions");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuestionsByCourseAndLevelConfirm(string course, string level)
        {
            try
            {
                var tokenstring = Request.Cookies["jwt"];
                var apiClient = _clientFactory.CreateClient("API");
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
                var response = await apiClient.DeleteAsync($"/api/Questions/byCourseAndLevel/{course}/{level}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"Questions for course '{course}' and '{level}' deleted successfully.";
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = $"Questions for course '{course}' and '{level}' not found. Status Code: {response.StatusCode}";
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    TempData["Error"] = $"Bad request. Please check your input. Status Code: {response.StatusCode}";
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["Error"] = $"Unauthorized access. Status Code: {response.StatusCode}";
                }
                else
                {
                    TempData["Error"] = $"Failed to delete questions for course '{course}' and level '{level}'. Status Code: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An unexpected error occurred: {ex.Message}";
            }

            return RedirectToAction("RemoveQuestions");
        }


    }
}

