using Mark2API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mark2API.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly OnlineDbContext _context;

        public UserRepository(OnlineDbContext context)
        {
            _context = context;
        }

        public IEnumerable<User> GetUsers()
        {
            return _context.Users.ToList();
        }

        public User GetUserById(int id)
        {
            return _context.Users.Find(id);
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public IEnumerable<User> GetUsersByEmail(string email)
        {
            return _context.Users.Where(u => u.Email == email).ToList();
        }
        public void CreateUser(User user)
        {
            // Hash and salt the user's password
            string salt = BCrypt.Net.BCrypt.GenerateSalt(12); // Generate a salt with 12 rounds (adjust as needed)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, salt);

            // Replace the user's plain-text password with the hashed password before saving to the database
            user.Password = hashedPassword;

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateUser(int id, User user)
        {
            if (id != user.Reg_Id)
            {
                throw new InvalidOperationException("Invalid user ID");
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    throw new InvalidOperationException("User not found");
                }
                else
                {
                    throw;
                }
            }
        }

        public void DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Reg_Id == id);
        }

        public bool ConfirmReset(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return false; // User not found
            }

            // Hash and salt the new password
            //the number represent cost factor in order to higher complexity but make it slow
            string salt = BCrypt.Net.BCrypt.GenerateSalt(12); // Generate a salt with 12 rounds (adjust as needed)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);// it salt the password based on the bcrypt alogorithm

            // Update the user's password
            user.Password = hashedPassword;
            _context.SaveChanges();

            return true; // Password updated successfully
        }

    }
}
