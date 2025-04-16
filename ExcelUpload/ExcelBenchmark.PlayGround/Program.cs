using BenchmarkDotNet.Running;
using ExcelBenchmark.PlayGround.UserServiceApp;
using System;

namespace ExcelBenchmark.PlayGround
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<ExcelBenchmark>();

            // For specific method test
            //var bench = new ExcelBenchmark();
            //bench.Setup();
            //bench.ClosedXml_Write(); 

            IUserService userService = new UserService();

            // Create a new user (normal creation, isClone default is false)
            var user = new User
            {
                ClientId = Guid.NewGuid(),
                ClientName = "OriginalClient",
                Email = "original@example.com"
            };
            userService.Create(user);  // No need to pass isClone, default is false

            // Clone an existing user
            userService.Clone(user.ClientId, "ClonedClient");
        }
    }
}
