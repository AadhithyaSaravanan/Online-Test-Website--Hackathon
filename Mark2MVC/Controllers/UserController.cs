using Mark2MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using DNTCaptcha.Core;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;

namespace Mark2MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IDNTCaptchaValidatorService _captchaValidator;
        private readonly DNTCaptchaOptions  _captchaOptions;

        public UserController(IHttpClientFactory clientFactory, IDNTCaptchaValidatorService captchaValidator, IOptions<DNTCaptchaOptions> captchaOptions)
        {
            _clientFactory = clientFactory;
            _captchaValidator = captchaValidator;
            _captchaOptions = captchaOptions == null ? throw new ArgumentNullException(nameof(captchaOptions)) : captchaOptions.Value;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Register(User user)
        {
            if (!_captchaValidator.HasRequestValidCaptchaEntry())
            {
                this.ModelState.AddModelError(_captchaOptions.CaptchaComponent.CaptchaInputName, "Please enter the security code as a number.");
                return View(user); // Return the registration view with an error message
            }
            var apiUrl = "https://localhost:7138/api/Users";
            using var client = _clientFactory.CreateClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration Successfully Done";
                // Registration successful, redirect to login page
                return RedirectToAction();
            }
            else
            {
                // Handle registration failure, e.g., display an error message
                TempData["ErrorMessage"] = "You are already Registered User. Please login.";
                return View(user);
            }


        }

        [HttpGet]

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Login(User LoginModel)
        {
            var apiUrl = "https://localhost:7138/api/Token";
            using var client = _clientFactory.CreateClient(apiUrl);
            // Convert the LoginModel object to JSON and create a StringContent for the HTTP request

            var jsonContent = new StringContent(JsonConvert.SerializeObject(LoginModel), Encoding.UTF8, "application/json");
            // Send a POST request to the token endpoint with the user's credentials
            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                var responseContent = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON response content into a JObject // can also extact and use as single propert from a group
                var tokenResponse = JsonConvert.DeserializeObject<JObject>(responseContent);
                if (tokenResponse.TryGetValue("token", out var tokenValue) && tokenValue.Type == JTokenType.String)
                {
                    // Extract the JWT token value
                    var token = tokenValue.Value<string>();

                   
                    // Assuming you have token and userEmail as variables containing the JWT token and user's email respectively

                    var userEmail = LoginModel.Email;
                    var jwtToken = token;

                    // Store the token in a separate cookie named "jwt"
                    Response.Cookies.Append("jwt", jwtToken, new CookieOptions
                    {
                        HttpOnly = true,//avoid the client side attacks
                        SameSite = SameSiteMode.Strict,// it is to confirm whether it gets from same site
                        Expires = DateTime.UtcNow.AddHours(1) // Set the expiration as needed
                    });

                    // Store the user email in a cookie named "userData"
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        Response.Cookies.Append("userData", userEmail, new CookieOptions
                        {
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddHours(1) // Set the expiration as needed
                        });
                    }



                }
                if (LoginModel.Email == "admin123@gmail.com" && LoginModel.Password == "Admin@123")
                {

                    // Redirect to the admin view
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                // Handle login failure, e.g., display an error message
                ModelState.AddModelError(string.Empty, "Login failed.");
                return View(LoginModel);
            }
        }
        //Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear the JWT token from the cookie
            Response.Cookies.Delete("jwt");
            Response.Cookies.Delete("userData");

            // Redirect to the login page
            return RedirectToAction("Login");
        }
    }
}
