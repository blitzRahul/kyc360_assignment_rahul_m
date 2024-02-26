using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using kyc360_assignment_rahul_m.Models;
using kyc360_assignment_rahul_m.Repositories;

namespace kyc360_assignment_rahul_m.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAPIController : ControllerBase
    {
        //provides access to the mock data layer
        private readonly IEntityRepository _entityRepository;
        //logger used for logging
        private readonly ILogger<TestAPIController> _logger;
        private int initial_wait_time = 2; // Initial wait time in seconds for retry delay

        public TestAPIController(IEntityRepository entityRepository, ILogger<TestAPIController> logger)
        {
            //_entityRepository is populated via dependency injection
            _entityRepository = entityRepository;
            _logger = logger;
        }

        // Endpoint to retrieve entities with optional query parameters
        //this function is async to support retry logic
        [HttpGet]
        public async Task<IActionResult> GetEntities([FromQuery] EntityQueryParameters queryParameters)
        {
            try
            {
                // Retry operation if it fails, then return entities
                var entities = await RetryOperation(() => _entityRepository.GetEntities(queryParameters), "SEARCH_BY_QUERY_PARAMETERS");
                return Ok(entities);
            }
            catch (Exception ex)
            {
                // Log and return error if operation fails
                _logger.LogError(ex, "Error retrieving entities");
                return StatusCode(500, "Internal server error");
            }
        }

        // Endpoint to retrieve a single entity by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntity(int id)
        {
            try
            {
                // Retry operation if it fails, then return the entity
                var entity = await RetryOperation(() => _entityRepository.GetEntityById(id), "GET_BY_ID");
                //if entity is not found respond with 404
                if (entity == null)
                {
                    return NotFound();
                }
                return Ok(entity);
            }
            catch (Exception ex)
            {
                // Log and return error if operation fails
                _logger.LogError(ex, $"Error retrieving entity with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // Endpoint to create a new entity
        //data for entity object is acquired from the body of the request
        [HttpPost]
        public async Task<IActionResult> CreateEntity([FromBody] Entity entity)
        {
            try
            {
                // Check if entity data is valid
                if (entity == null)
                {
                    return BadRequest("Entity data is null");
                }

                // Retry operation if it fails, then return the created entity
                var createdEntity = await RetryOperation(() => _entityRepository.CreateEntity(entity), "CREATE");
                return CreatedAtAction(nameof(GetEntity), new { id = createdEntity.id }, createdEntity);
            }
            catch (Exception ex)
            {
                // Log and return error if operation fails
                _logger.LogError(ex, "Error creating entity");
                return StatusCode(500, "Internal server error");
            }
        }

        // Endpoint to update an existing entity
        //data for entity object is acquired from the body of the request
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntity(int id, [FromBody] Entity entity)
        {
            try
            {
                // Check if entity data is valid
                if (entity == null || id != entity.id)
                {
                    return BadRequest("Invalid entity data. Check query and body.");
                }

                // Retrieve entity by ID
                var existingEntity = await RetryOperation(() => _entityRepository.GetEntityById(id), "GET_BY_ID");
                //if it is null then respond with 404
                if (existingEntity == null)
                {
                    return NotFound();
                }

                // Retry operation if it fails, then return the updated entity
                var updatedEntity = await RetryOperation(() => _entityRepository.UpdateEntity(entity), "UPDATE");
                return Ok(updatedEntity);
            }
            catch (Exception ex)
            {
                // Log and return error if operation fails
                _logger.LogError(ex, $"Error updating entity with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // Endpoint to delete an existing entity
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntity(int id)
        {
            try
            {
                // Retrieve the existing entity by ID
                var existingEntity = await RetryOperation(() => _entityRepository.GetEntityById(id), "GET_BY_ID");
                //if entity is not found respond with 404
                if (existingEntity == null)
                {
                    return NotFound();
                }

                // Retry operation if it fails, then delete the entity
                await RetryOperation(async () =>  _entityRepository.DeleteEntity(id) ,"DELETE");
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log and return error if operation fails
                _logger.LogError( $"Error deleting entity with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // Retry operation with exponential backoff strategy
        private async Task<T> RetryOperation<T>(Func<Task<T>> operation,string operation_name, int maxAttempts = 3)
        {
            int attempt = 0;
            TimeSpan delay = TimeSpan.FromSeconds(initial_wait_time);

            while (true)
            {
                try
                {
                    // Try executing the operation
                    return await operation();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.Message);
                    attempt++;
                    if (attempt >= maxAttempts)
                    {
                        // Maximum retry attempts reached, throw the exception
                        throw;
                    }
                    // Log the retry attempt and wait before the next attempt
                    _logger.LogWarning($"Retry attempt {attempt} for operation: {operation_name}");
                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds((int)Math.Pow(initial_wait_time, attempt)); // Exponential backoff
                }
            }
        }
    }
}
