using Mark2API.Models;
using System;
using System.Collections.Generic;

namespace Mark2API.Repository
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User GetUserById(int id);
        User GetUserByEmail(string email);
        IEnumerable<User> GetUsersByEmail(string email);//forget
        void CreateUser(User user);
        void UpdateUser(int id, User user);
        void DeleteUser(int id);
        bool UserExists(int id);
        bool ConfirmReset(string email, string password);
    }
}
