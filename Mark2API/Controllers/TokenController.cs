using Mark2API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Mark2API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly OnlineDbContext _con;
        private readonly IConfiguration _configuration;

        public TokenController(OnlineDbContext con, IConfiguration config)
        {
            _con = con;
            _configuration = config;
        }

        [HttpPost]
        public async Task<IActionResult> Post(User.LoginRequest _userData)
        {
            if (_userData != null && _userData.Email != null && _userData.Password != null)
            {
                User user = null;

                // Check if the email and password match the hardcoded admin credentials
                if (_userData.Email == "admin123@gmail.com" && _userData.Password == "Admin@123")
                {
                    user = new User
                    {
                        Email = "admin123@gmail.com",
                        Password = "Admin@123",
                    };
                }
                else
                {
                    user = await GetUser(_userData.Email, _userData.Password);
                }

                if (user != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("Email", user.Email),
                new Claim("Password", user.Password),
            };

                    // Conditionally add role claims based on user type
                    if (user.Email == "admin123@gmail.com" && user.Password == "Admin@123")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "User"));
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(120),
                        signingCredentials: signIn);

                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<User> GetUser(string email, string password)
        {
            // Retrieve the user by email
            var user = await _con.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Check if the user exists and if the provided password matches the stored hashed password
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }

            return null; // Return null if the user is not found or the password doesn't match
        }
    }
}