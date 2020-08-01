using System;
using System.Net.Http;
using System.Threading.Tasks;
using TandemUserService;

namespace TandemUserServiceIntegrationTests
{
    public class Program
    {
        public static readonly HttpClient _client = new HttpClient();

        public static async void Main(string[] args)
        {
            Console.WriteLine("Tandem user Service Integration Tests! \r\n");
            //jeb
        }

        public static async Task TestGetUri(string uri)
        {
            try
            {
                var response = await _client.GetAsync(uri);
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