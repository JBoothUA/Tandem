using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TandemUserService.Models;

namespace TandemUserService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "Tandem";
        private string containerId = "Users";

        public UserController(ILogger<UserController> logger,IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<TandemUser> Get()
        {
            //var response = await cosmosClient.GetContainer(_dbname, _container)
            //    .CreateItemAsync(item, null, new ItemRequestOptions
            //        { PostTriggers = new List<string> { "validateSoldItem" } });
            //jeb return response.RequestCharge;

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new TandemUser
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55)
            })
            .ToArray();
        }

        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of emailAddress in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task QueryUsersAsync(string emailAddress)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.emailAddress = '{emailAddress}'";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<User> queryResultSetIterator = this.container.GetItemQueryIterator<User>(queryDefinition);

            List<User> families = new List<User>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<User> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (User family in currentResultSet)
                {
                    families.Add(family);
                    Console.WriteLine("\tRead {0}\n", family);
                }
            }
        }











        [HttpPost]
        public IEnumerable<TandemUser> Create()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new TandemUser
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55)
            })
            .ToArray();
        }
    }
}
