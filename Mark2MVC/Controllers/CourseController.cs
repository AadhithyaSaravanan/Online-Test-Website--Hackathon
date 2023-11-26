using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Mark2MVC.Models;
using System.Net.Http.Formatting;
using System.Drawing;
using System.Net;


namespace Mark2MVC.Controllers
{
    public class CourseController : Controller
    {
        private readonly IHttpClientFactory _clientfactory;

        public CourseController(IHttpClientFactory clientfactory)
        {
            _clientfactory = clientfactory;
        }

        public async Task<IActionResult> CourseIndex()
        {
            var tokenstring = Request.Cookies["jwt"];
            var client = _clientfactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
            var response = await client.GetAsync("/api/Courses");
            if (response.IsSuccessStatusCode)
            {
                var courses = await response.Content.ReadFromJsonAsync<IEnumerable<Course>>();
                return View(courses);
            }
            return View(null);
        }
        // GET: CategoryController/Details/5
        public async Task<ActionResult<Course>> Details(int id)
        {
            var tokenstring = Request.Cookies["jwt"];
            var client = _clientfactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);
            var response = await client.GetAsync($"/api/Courses/{id}");

            var courses = await response.Content.ReadFromJsonAsync<Course>();
            return View(courses);
        }

        // GET: CategoryController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CategoryController/Create
        [HttpPost]
        public async Task<IActionResult> Create(Course course, IFormFile Image)
        {
            var tokenstring = Request.Cookies["jwt"];

            var client = _clientfactory.CreateClient("API");
            //If an image is provided in the form, it reads the image content into a byte array and assigns it to the course.Image property.
            if (Image != null && Image.Length > 0)
            {
                using (var stream = new MemoryStream())//creating a temporary storage to hold
                {
                    await Image.CopyToAsync(stream);//copy the file
                    course.Image = stream.ToArray();//convert image to array

                }
            }

            var content = new StringContent(JsonConvert.SerializeObject(course), Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);

            HttpResponseMessage response = await client.PostAsync("/api/Courses", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CourseIndex");
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = errorMessage;
                }
                else
                {
                    return View("Error");
                }
                return View();
            }
        }

        // GET: CategoryController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {

            var client = _clientfactory.CreateClient("API");
            var response = await client.GetAsync($"/api/Courses/{id}");
            var course = await response.Content.ReadFromJsonAsync<Course>();
            return View(course);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Course course, IFormFile Image)
        {
            var tokenstring = Request.Cookies["jwt"];

            var client = _clientfactory.CreateClient("API");
            if (Image != null && Image.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await Image.CopyToAsync(stream);
                    course.Image = stream.ToArray();

                }
            }
            var packageJson = JsonConvert.SerializeObject(course);

            var content = new StringContent(JsonConvert.SerializeObject(course), Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);

            var response = await client.PutAsync($"/api/Courses/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CourseIndex");
            }
            else
            {
                return View("Error");
            }
        }

        public async Task<ActionResult> Delete(int id)
        {
            var client = _clientfactory.CreateClient("API");
            var response = await client.GetAsync($"/api/Courses/{id}");
            var course = await response.Content.ReadFromJsonAsync<Course>();
            return View(course);
        }

        // POST: CategoryController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            var tokenstring = Request.Cookies["jwt"];

            var client = _clientfactory.CreateClient("API");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenstring);


            var response = await client.DeleteAsync($"/api/Courses/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CourseIndex");
            }
            else
            {
                return View("Error");
            }
        }

    }
}