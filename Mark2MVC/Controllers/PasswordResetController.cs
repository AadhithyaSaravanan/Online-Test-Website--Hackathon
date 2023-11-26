using Microsoft.AspNetCore.Mvc;
using Mark2MVC.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json; // You may need this for JSON serialization
using MimeKit;
using MailKit.Net.Smtp;
using System.Text;
using static Mark2MVC.Models.User;

namespace Mark2MVC.Controllers
{
    public class PasswordResetController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public PasswordResetController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(User.PasswordReset model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                TempData["ResetPasswordError"] = "Please Enter your Email ID";
                return RedirectToAction("ForgotPassword");
            }

            if (ModelState.IsValid)
            {
                var httpClient = _clientFactory.CreateClient("API");
                var response = await httpClient.GetAsync($"api/Users/GetUserByEmail?email={model.Email}");

                if (response.IsSuccessStatusCode)
                {
                    var userJson = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<User>(userJson);

                    if (user != null)
                    {
                        var emailService = new EmailService();

                        var resetLink = GenerateResetLink(model);

                        await emailService.SendEmailAsync(user.Email, "Password Reset Request for Your Account", user.FullName, resetLink);//the heading

                        TempData["ResetPasswordEmail"] = model.Email;

                        TempData["ResetPasswordNote"] = "The reset link has been sent to your email address.";

                    }
                }
                else
                {
                    TempData["ResetPasswordNote"] = "You have not registered with us.";
                }
            }

            return RedirectToAction("ForgotPassword");
        }

        public class EmailService
        {
            private readonly string smtpHost = "smtp.gmail.com";//default email for smtp
            private readonly int smtpPort = 465;//default port number
            private readonly string smtpUsername = "chandrubhaskar51@gmail.com";//email
            private readonly string smtpPassword = "shijnuwrjllopfxk";//app password

            public async Task SendEmailAsync(string toEmail, string subject, string userName, string resetLink)
            {
                var message = new MimeMessage();// Creates a new MimeMessage object, which represents an email message.
                message.From.Add(new MailboxAddress("Online Exam", smtpUsername));// Sets the sender's email address.
                message.To.Add(new MailboxAddress("", toEmail));//Sets the user email address.
                message.Subject = subject;// Sets the subject of the email.

                // Create a user-friendly email body with the user's name and reset link
                var body = $"Dear {userName},\n\n";
                body += $"A password reset link has been sent to your email address for resetting your password.\n\n";
                body += $"Please click on the following link to reset your password:\n\n";
                body += $"{resetLink}\n\n";
                body += $"If you did not request this password reset, please ignore this email.\n\n";
                body += $"Thank you for using our service.\n\n";
                body += $"Best regards,\n";
                body += $"The Online Exam Team";

                var textPart = new TextPart("plain")// Creates a text part of the email with the user-friendly message. without any formatting like design
                {
                    Text = body
                };

                var multipart = new Multipart("mixed");//Creates a Multipart object to represent the entire email content. also as plain text and attachment
                multipart.Add(textPart);//Creates a Multipart object to represent the entire email content.

                message.Body = multipart;// Sets the email body to the multipart content. like maincontent

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpHost, smtpPort, true);// connect with smtp
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);// authorize email and password
                    await client.SendAsync(message);// send the email
                    await client.DisconnectAsync(true);// disconnect from the server of email smtp
                }
            }

        }



        private string GenerateResetLink(User.PasswordReset user)//private method to generate link
        {
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(120); // Set expiration to 10 minutes from now
            string resetToken = Guid.NewGuid().ToString();// new reset token for every time
            string resetLink = $"https://localhost:7183/PasswordReset/ResetPassword?token={resetToken}&email={user.Email}";
            user.ResetToken = resetToken;//sets reset token property for the user
            return resetLink;// return the constructed link
        }
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ConfirmReset
            {
                ResetToken = token,//getting token and emailfrom return from the genrated link
                Email = email 
            };
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ConfirmReset model)
        {
            if (ModelState.IsValid)
            {
                // Create an instance of HttpClient
                using (var httpClient = _clientFactory.CreateClient())
                {
                    string apiUrl = "https://localhost:7138/api/Users/ConfirmReset";
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["ResetPasswordMessage"] = "Password has been reset successfully.";
                    }
                    else
                    {
                        // Password reset failed
                        TempData["ResetPasswordMessage"] = "Password reset failed. Please try again.";

                        // Log the response content for debugging
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Password reset error: {errorContent}");
                    }
                }
            }

            return View(model);
        }
    }
}

