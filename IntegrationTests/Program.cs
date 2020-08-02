using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TandemUserService;

namespace TandemUserServiceIntegrationTests
{
    public class Program
    {
        public static readonly HttpClient _client = new HttpClient();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Start the Tandem User Service Integration Tests!");
            Console.WriteLine("Please ensure that the User Service API is running first! \n");

            Console.WriteLine("Attempt a Health Check for the Data Store.");
            Console.WriteLine("Expect a 200 Success with an info message.");
            await TestGetUri("https://localhost:44360/api/v1/user/test@test.com");
            Console.ReadLine();

            Console.WriteLine("Attempt to get a user that doesn't exist.");
            Console.WriteLine("Expect a 404 Not Found error below.");
            await TestGetUri("https://localhost:44360/api/v1/user/does-not-exist@do-not-add.com");
            Console.ReadLine();

            Console.WriteLine("Attempt to create a new user with email test@test.com.");
            Console.WriteLine("Expect a 201 Success with a new user.");
            var user = new TandemUser
            {
                 EmailAddress = "test@test.com",
                 FirstName = "Jesse",
                 LastName = "Booth",
                 MiddleName = "Evan",
                 PhoneNumber = "256-797-6092"
            };
            await TestPostUri("https://localhost:44360/api/v1/user", user);
            Console.ReadLine();

            Console.WriteLine("Attempt to create a new user with email testtestcom.");
            Console.WriteLine("Expect a 400 Bad Request error below, due to an invalid email address.");
            user = new TandemUser
            {
                EmailAddress = "testtestcom",
                FirstName = "Jesse",
                LastName = "Booth"
            };
            await TestPostUri("https://localhost:44360/api/v1/user", user);
            Console.ReadLine();

            Console.WriteLine("Attempt to get the user that we created (test@test.com).");
            Console.WriteLine("Expect a 200 Success with the existing user.");
            await TestGetUri("https://localhost:44360/api/v1/user/test@test.com");
            Console.ReadLine();
        }

        public static async Task TestGetUri(string uri)
        {
            try
            {
                Console.WriteLine(await _client.GetStringAsync(uri));
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("\nMessage: {0} ", e.Message);
            }
        }

        public static async Task TestPostUri(string uri, TandemUser user)
        {
            try
            {
                var content = new StringContent(user.ToString(), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("\nMessage: {0} ", e.Message);
            }
        }
    }
}