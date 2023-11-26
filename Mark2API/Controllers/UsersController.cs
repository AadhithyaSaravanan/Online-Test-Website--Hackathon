using Microsoft.AspNetCore.Mvc;
using Mark2API.Models;
using Mark2API.Repository;
using System;
using System.Collections.Generic;

namespace Mark2API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            var users = _userRepository.GetUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }


        [HttpGet("GetUserByEmail")]// it is to retrive and store it in the result page
        public ActionResult<User> GetUserByEmail([FromQuery] string email)
        {
            var user = _userRepository.GetUserByEmail(email);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }


        [HttpGet("GetUsersByEmail/{email}")]//forget check whether the record is available
        public IActionResult GetUsersByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email parameter is required.");
            }

            var users = _userRepository.GetUsersByEmail(email);

            if (users == null || !users.Any())
            {
                return NotFound($"No records found for email: {email}");
            }

            return Ok(users);
        }




        [HttpPost]//registration
        public IActionResult PostUser(User user)
        {
            try
            {
                _userRepository.CreateUser(user);
                return CreatedAtAction("GetUser", new { id = user.Reg_Id }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]//exception/option
        public IActionResult PutUser(int id, User user)
        {
            try
            {
                _userRepository.UpdateUser(id, user);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return BadRequest("Invalid user ID");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]//exception/option
        public IActionResult DeleteUser(int id)
        {
            try
            {
                _userRepository.DeleteUser(id);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ConfirmReset")]
        public IActionResult ConfirmReset([FromBody] User.ConfirmReset model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Invalid request data");
            }

            _userRepository.ConfirmReset(model.Email, model.Password);

            return Ok("Password update process initiated");
        }
    }
}
