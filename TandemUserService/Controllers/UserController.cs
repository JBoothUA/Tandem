using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TandemUserService.Configuration;

namespace TandemUserService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserServiceConfig _userServiceConfig;

        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;

        private string databaseId = "Tandem";
        private string containerId = "Users";

        public UserController(ILogger<UserController> logger, IOptions<UserServiceConfig> config)
        {
            _logger = logger;
            _userServiceConfig = config.Value;

            _cosmosClient = new CosmosClient(_userServiceConfig.UserDataStoreEndPointUri, _userServiceConfig.UserDataStorePrimaryKey, 
                new CosmosClientOptions() { ApplicationName = "Tandem User Service" });
            _database = _cosmosClient.GetDatabase(databaseId);
            _container = _database.GetContainer(containerId);
        }

        [HttpGet]
        public async Task<TandemUser> Get()
        {
            var user = await QueryUsersByEmailAddress("mary@elitechildcare.com");
            return user;
        }

        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of emailAddress in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task<TandemUser> QueryUsersByEmailAddress(string emailAddress)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.emailAddress = '{emailAddress}'";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.GetItemQueryIterator<TandemUser>(queryDefinition);
            var currentResultSet = await queryResultSetIterator.ReadNextAsync();
            return currentResultSet.Resource.Count() > 0 ? currentResultSet.Resource.First() : null;
        }











        [HttpPost]
        public IEnumerable<TandemUser> Create()
        {
            return null;
        }
    }
}
