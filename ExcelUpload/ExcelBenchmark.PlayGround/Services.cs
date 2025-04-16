using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelBenchmark.PlayGround
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace UserServiceApp
    {
        // User model
        public class User
        {
            public Guid ClientId { get; set; }
            public string ClientName { get; set; }
            public string Email { get; set; }
        }

        // In-memory DB simulation
        public static class UserRepository
        {
            public static List<User> Users = new List<User>();
        }

        public interface IUserService
        {
            void Create(User user, bool isClone = false);
            void Clone(Guid existingClientIdToClone, string newClientName);
        }

        public class UserService : IUserService
        {
            // Create method (directly from interface)
            public void Create(User user, bool isClone)
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                ValidateUser(user, isClone);

                UserRepository.Users.Add(user);
                Console.WriteLine($"User {(isClone ? "cloned" : "created")} successfully: {user.ClientName}");
            }

            // Clone method
            public void Clone(Guid existingClientIdToClone, string newClientName)
            {
                var existingUser = UserRepository.Users.FirstOrDefault(u => u.ClientId == existingClientIdToClone);

                if (existingUser == null)
                    throw new ArgumentException("User to clone not found.");

                var clonedUser = new User
                {
                    ClientId = Guid.NewGuid(),  // New unique ClientId
                    ClientName = newClientName,
                    Email = existingUser.Email
                };

                // Call Create with isClone = true
                Create(clonedUser, isClone: true);
            }

            // Private validation method
            private void ValidateUser(User user, bool isClone)
            {
                if (!isClone)
                {
                    if (user.ClientId == Guid.Empty)
                        throw new ArgumentException("ClientId is required for creation.");

                    if (string.IsNullOrWhiteSpace(user.ClientName))
                        throw new ArgumentException("ClientName is required for creation.");

                    if (string.IsNullOrWhiteSpace(user.Email))
                        throw new ArgumentException("Email is required for creation.");
                }

                // Common validation for both create and clone
                if (string.IsNullOrWhiteSpace(user.ClientName))
                    throw new ArgumentException("ClientName cannot be empty.");

                if (UserRepository.Users.Any(u => u.ClientName.Equals(user.ClientName, StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("ClientName must be unique.");
            }
        }

        // Example usage
        //class Program
        //{
        //    static void Main(string[] args)
        //    {

        //    }
        //}
    }

}
