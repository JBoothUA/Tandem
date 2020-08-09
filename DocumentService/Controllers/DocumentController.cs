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
using TandemUserService.Configuration;//jeb

namespace TandemUserService.Controllers//jeb
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;
        private readonly IMapper _mapper;
        private readonly UserServiceConfig _userServiceConfig;

        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly Container _container;

        private readonly string databaseId = "Tandem";
        private readonly string containerId = "Users";

        public DocumentController(ILogger<DocumentController> logger, IOptions<UserServiceConfig> config)
        {
            _logger = logger;
            _userServiceConfig = config.Value;

            try
            {
                var mapConfig = new MapperConfiguration(cfg => {
                    cfg.CreateMap<TandemUser, TandemUserDto>()
                      .ForMember(dest => dest.UserId,
                            opts => opts.MapFrom(src => src.Id))
                      .ForMember(dest => dest.Name,
                            opts => opts.MapFrom(src => $"{src.FirstName} {src.MiddleName} {src.LastName}"));
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

        // GET /api/v1/user/healthcheck
        /// <summary>
        /// Health Check on the Persistent Store Connectivity
        /// </summary>
        [HttpGet]
        [Route("healthcheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> HealthCheck()
        {
            try
            {
                await _cosmosClient.ReadAccountAsync().ConfigureAwait(false);
            }
            catch
            {
                return Ok("PROBLEM! - Persistent Store Connectivity Health Check");
            }

            return Ok("SUCCESS! - Persistent Store Connectivity Health Check");
        }

        // GET /api/v1/user/mary@elitechildcare.com
        /// <summary>
        /// Run a query against the Users container including the value of emailAddress in the WHERE filter
        /// </summary>
        [HttpGet]
        [Route("{emailAddress}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TandemUserDto>>> Get(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                return BadRequest();
            }
            
            var users = await GetUsersByEmailAddressAsync(emailAddress).ConfigureAwait(false);
            if (users == null || users.Count() == 0)
            {
                return NotFound();
            }

            var tandemUsers = new List<TandemUserDto>();
            foreach (var user in users)
            {
                tandemUsers.Add(_mapper.Map<TandemUser, TandemUserDto>(user));
            }

            return Ok(tandemUsers);
        }

        private async Task<IEnumerable<TandemUser>> GetUsersByEmailAddressAsync(string emailAddress)
        {
            try
            {
                var sqlQueryText = $"SELECT * FROM c WHERE c.emailAddress = '{emailAddress}'";

                var queryDefinition = new QueryDefinition(sqlQueryText);
                var queryResultSetIterator = _container.GetItemQueryIterator<TandemUser>(queryDefinition);
                var currentResultSet = await queryResultSetIterator.ReadNextAsync().ConfigureAwait(false);
                return currentResultSet.Resource != null && currentResultSet.Resource.Count() != 0 ? currentResultSet.Resource : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }

        // POST /api/v1/user
        /// <summary>
        /// Add User items to the container
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TandemUser>> CreateUser([FromBody] TandemUser user)
        {
            return CreatedAtAction(nameof(CreateUser), await AddUserToContainerAsync(user).ConfigureAwait(false));
        }

        private async Task<TandemUser> AddUserToContainerAsync(TandemUser user)
        {
            user.EnsureId();
            return await _container.CreateItemAsync(user, new PartitionKey(user.EmailAddress)).ConfigureAwait(false);
        }
    }
}
