using Microsoft.AspNetCore.Mvc;
using Mark2MVC.Models;
using System.Net.Http.Headers;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using iTextSharp.text.pdf;
using iTextSharp.text;
using MailKit.Search;
using NuGet.Protocol;

namespace Mark2MVC.Controllers
{
    public class TestController : Controller
    {
        private readonly IHttpClientFactory _clientfactory;

        public TestController(IHttpClientFactory clientfactory)
        {
            _clientfactory = clientfactory;
        }
        public async Task<IActionResult> Index()//retriving test data from table and displaying
        {
            var tokenstring = Request.Cookies["jwtToken"];
            var client = _clientfactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
            var response = await client.GetAsync("/api/TestResult");
            

            if (response.IsSuccessStatusCode)
            {
                var courses = await response.Content.ReadFromJsonAsync<IEnumerable<TestResultReport>>();
                // Extract distinct course names, states, and cities from the data for the drop down
                var distinctCourseNames = courses.Select(c => c.CourseName).Distinct().ToList();
                var distinctStates = courses.Select(c => c.State).Distinct().ToList();
                var distinctCities = courses.Select(c => c.City).Distinct().ToList();

                // Store these distinct values in ViewData
                ViewData["CourseNames"] = new SelectList(distinctCourseNames);
                ViewData["States"] = new SelectList(distinctStates);
                ViewData["Cities"] = new SelectList(distinctCities);
                return View(courses);
            }
            return View(new List<TestResultReport>());
        }


        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> GetByEmail()//retriving test details for the users
        {
            // Get user email from cookies
            var email = Request.Cookies["userData"];

            var client = _clientfactory.CreateClient("API");
            var response = await client.GetAsync($"/api/TestResult/GetByEmail?email={email}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<List<TestResultReport>>(jsonString);

                if (user.Any())
                {
                    return View("GetByEmail", user); // Pass the user details to the Index view
                }
                else
                {
                    var errorMessage = "No records found for the provided email.";
                    TempData["ErrorMessage"] = errorMessage; // Store the error message in TempData
                    return View("GetByEmail"); // Display the TempData message in the view
                }
            }
            else
            {
                // Handle API call failure here
                var errorMessage = "Failed to retrieve data. Please try again later.";
                TempData["ErrorMessage"] = errorMessage; // Store the error message in TempData
                return View("GetByEmail"); // Redirect back to the Index action with error message
            }
        }
        public async Task<IActionResult> Search(string courseName, string state, string city, string level, int? marks)
        {
            try
            {
                

                if (string.IsNullOrWhiteSpace(courseName) && string.IsNullOrWhiteSpace(state) && string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(level) && !marks.HasValue)
                {
                    // All search parameters are empty, display an error message
                    ViewBag.ErrorMessage = "At least one field is required to perform a search.";
                    return View(new List<TestResultReport>());
                }

                var client = _clientfactory.CreateClient("API");

                string requestUri = $"api/TestResult/Search?courseName={courseName}&state={state}&city={city}&level={level}&marks={marks}";
                HttpResponseMessage response = await client.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    List<TestResultReport> searchResults = await response.Content.ReadAsAsync<List<TestResultReport>>();
                    return View(searchResults);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // No records found
                    ViewBag.ErrorMessage = "No records found matching the search criteria.";
                    return View(new List<TestResultReport>());
                }
                else
                {
                    ViewBag.ErrorMessage = "An error occurred while processing your request.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while processing your request: " + ex.Message;
                return View();
            }
        }
       
        public IActionResult GeneratePDF(List<TestResultReport> searchResults)
        {
            if (searchResults != null && searchResults.Count > 0)
            {
                // Create a new Document
                Document doc = new Document();
                MemoryStream memoryStream = new MemoryStream();//This creates a MemoryStream to which the PDF document will be written.
                PdfWriter writer = PdfWriter.GetInstance(doc, memoryStream);//It's responsible for writing the content of the PDF document to the stream.

                doc.Open();

                // Title and Subtitle
                Font titleFont = new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD);

                // Add title and center align it
                Paragraph title = new Paragraph("Search Results", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);

                // Create a PdfPTable with 8 columns
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;

                table.SpacingBefore = 20;
                table.SpacingAfter = 20;

                Font headerFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.BLACK); // Set the header font and color

                // Define the column widths for each column individually
                float[] columnWidths = new float[] { 1.5f, 3.8f, 1.8f, 2f, 1.5f, 1.6f, 1.2f, 1f };

                // Set the column widths
                table.SetWidths(columnWidths);

                // Add table headers with gray color and center alignment
                PdfPCell headerCell;
                headerCell = new PdfPCell(new Phrase("Name", headerFont));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(headerCell);

                headerCell = new PdfPCell(new Phrase("Email", headerFont));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(headerCell);

                headerCell = new PdfPCell(new Phrase("Mobile", headerFont));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(headerCell);

                headerCell = new PdfPCell(new Phrase("State", headerFont));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(headerCell);

                headerCell = new PdfPCell(new Phrase("City", headerFont));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(headerCell);

                headerCell = new PdfPCell(new Phrase("Course Name", headerFont));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(headerCell);

                headerCell = new PdfPCell(new Phrase("Marks", headerFont));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(headerCell);

                headerCell = new PdfPCell(new Phrase("Level", headerFont));
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(headerCell);

                // Add data rows from the search results with styling
                Font dataFont = new Font(Font.FontFamily.HELVETICA, 10);
                foreach (var result in searchResults)
                {
                    table.AddCell(new Phrase(result.FullName, dataFont));
                    table.AddCell(new Phrase(result.Email, dataFont));
                    table.AddCell(new Phrase(result.Mobile, dataFont));
                    table.AddCell(new Phrase(result.State, dataFont));
                    table.AddCell(new Phrase(result.City, dataFont));
                    table.AddCell(new Phrase(result.CourseName, dataFont));
                    table.AddCell(new Phrase(result.TotalMarks.ToString(), dataFont));
                    table.AddCell(new Phrase(result.Level, dataFont));
                }
                // Add the completed table to the document
                doc.Add(table);
                doc.Close();

                // Return the styled PDF as a file download
                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();

                return File(bytes, "application/pdf", "SearchResults.pdf");
            }
            else
            {
                // Handle the case where there are no search results
                // You can return an empty PDF or another response as needed
                return new EmptyResult();
            }
        }

    }

}