using System;
using System.Net.Http;
using System.Threading.Tasks;
using TandemUserService;

namespace TandemUserServiceIntegrationTests
{
    public class Program
    {
        public static readonly HttpClient _client = new HttpClient();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Start the Tandem User Service Integration Tests! \r\n");
            
            //jeb
            await TestGetUri("https://localhost:11111/api/v1/user/does-not-exist@do-not-add.com");
            await TestGetUri("https://localhost:11111/api/v1/user/com");
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
                Console.WriteLine("Message: {0} ", e.Message);
            }
        }

        public static async Task TestPostUri(string uri, TandemUser user)
        {
            try
            {
                var response = await _client.PostAsync(uri, null);//jeb user?
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message: {0} ", e.Message);
            }
        }
    }
}