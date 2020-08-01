using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TandemUserService.Configuration;

namespace TandemUserService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;
        private readonly UserServiceConfig _userServiceConfig;

        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly Container _container;

        private readonly string databaseId = "Tandem";
        private readonly string containerId = "Users";

        public UserController(ILogger<UserController> logger, IOptions<UserServiceConfig> config)
        {
            _logger = logger;
            _userServiceConfig = config.Value;

            try
            {
                var mapConfig = new MapperConfiguration(cfg => {
                    cfg.CreateMap<TandemUser, TandemUserDto>();
                });
                _mapper = mapConfig.CreateMapper();

                _cosmosClient = new CosmosClient(_userServiceConfig.UserDataStoreEndPointUri, _userServiceConfig.UserDataStorePrimaryKey,
                    new CosmosClientOptions() { ApplicationName = "Tandem User Service" });
                _database = _cosmosClient.GetDatabase(databaseId);
                _container = _database.GetContainer(containerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of emailAddress in the WHERE filter results in a more efficient query
        /// </summary>
        [HttpGet("emailAddress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TandemUserDto>> Get(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                return BadRequest();
            }
            
            var user = await GetUserByEmailAddress(emailAddress);
            if (user == null)
            {
                return NotFound();
            }

            return _mapper.Map<TandemUser, TandemUserDto>(user);
        }

        private async Task<TandemUser> GetUserByEmailAddress(string emailAddress)
        {
            try
            {
                var sqlQueryText = $"SELECT * FROM c WHERE c.emailAddress = '{emailAddress}'";

                var queryDefinition = new QueryDefinition(sqlQueryText);
                var queryResultSetIterator = _container.GetItemQueryIterator<TandemUser>(queryDefinition);
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                return currentResultSet.Resource.Count() > 0 ? currentResultSet.Resource.First() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Add User items to the container
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TandemUser>> CreateUser([FromBody] TandemUser user)
        {
            return await AddItemsToContainerAsync(user);
        }

        private async Task<TandemUser> AddItemsToContainerAsync(TandemUser user)
        {
           //jeb user.UserId = Guid.NewGuid();
            return await _container.CreateItemAsync(user);
        }
    }
}
