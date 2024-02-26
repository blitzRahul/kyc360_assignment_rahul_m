using Bogus;
using kyc360_assignment_rahul_m.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kyc360_assignment_rahul_m.Repositories
{
    // This class implements the IEntityRepository interface
    // It serves as a mock data layer simulating interaction with a database
    public class MockEntityRepository : IEntityRepository
    {
        // Holds all the mock entities (simulates database storage)
        private readonly List<Entity> _entities;

        // Used to generate dummy data
        private readonly Faker<Entity> _faker;

        // Number of dummy entries to generate
        private int entity_count = 50;

        // Flag to enable or disable database operation failure simulation
        private bool can_fail = true;

        // Probability of failure if enabled
        private double probability_failure = 0.5;

        // Random number generator
        private Random rnd;

        // Constructor initializes the faker and generates mock entities
        public MockEntityRepository()
        {
            rnd = new Random();
            _faker = new Faker<Entity>()
                .RuleFor(e => e.id, f => f.UniqueIndex) // Generates unique index
                .RuleFor(e => e.name, f => new Name { FirstName = f.Person.FirstName, MiddleName = f.Person.FirstName, Surname = f.Person.LastName }) // Generates fake name
                .RuleFor(e => e.gender, f => f.PickRandom("Male", "Female")) // Randomly selects male or female
                .RuleFor(e => e.address, f => new Address { AddressLine = f.Address.StreetAddress(), City = f.Address.City(), Country = f.PickRandom("USA", "UK", "India", "Canada", "Mexico") }) //picks a random country
                .RuleFor(e => e.date, f => new Date { DateType = "DateType", Date_T = f.Date.Past() }); // Generates fake date

            _entities = _faker.Generate(entity_count); // Generates mock entities
        }

        // Simulates retrieving entities from the database with optional filtering, sorting, and pagination
        public Task<IEnumerable<Entity>> GetEntities(EntityQueryParameters queryParameters)
        {
            FailureSimulation("GET_BY_QUERY"); // Simulates database operation failure

            // Converts _entities to IEnumerable for easy traversal
            var filteredEntities = _entities.AsEnumerable();

            // Filters entities based on search criteria
            if (!string.IsNullOrWhiteSpace(queryParameters.Search))
            {
                filteredEntities = filteredEntities.Where(entity =>
                    entity.name?.FirstName?.Contains(queryParameters.Search, StringComparison.OrdinalIgnoreCase) == true ||
                    entity.name?.MiddleName?.Contains(queryParameters.Search, StringComparison.OrdinalIgnoreCase) == true ||
                    entity.name?.Surname?.Contains(queryParameters.Search, StringComparison.OrdinalIgnoreCase) == true ||
                    entity.address?.AddressLine?.Contains(queryParameters.Search, StringComparison.OrdinalIgnoreCase) == true ||
                    entity.address?.Country?.Contains(queryParameters.Search, StringComparison.OrdinalIgnoreCase) == true
                );
            }

            // Filters entities based on gender
            if (!string.IsNullOrWhiteSpace(queryParameters.Gender))
            {
                filteredEntities = filteredEntities.Where(entity =>
                    entity.gender?.Equals(queryParameters.Gender, StringComparison.OrdinalIgnoreCase) == true
                );
            }

            // Filters entities based on start date
            if (queryParameters.StartDate.HasValue)
            {
                filteredEntities = filteredEntities.Where(entity =>
                    entity.date?.Date_T >= queryParameters.StartDate
                );
            }

            // Filters entities based on end date
            if (queryParameters.EndDate.HasValue)
            {
                filteredEntities = filteredEntities.Where(entity =>
                    entity.date?.Date_T <= queryParameters.EndDate
                );
            }

            // Filters entities based on countries
            if (queryParameters.Countries != null && queryParameters.Countries.Any())
            {
                filteredEntities = filteredEntities.Where(entity =>
                    queryParameters.Countries.Contains(entity.address?.Country, StringComparer.OrdinalIgnoreCase)
                );
            }

            // Sorts entities
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                switch (queryParameters.SortBy.ToLower())
                {
                    case "firstname":

                        //if  the sort order is descending sort filteredEntities in descending order, else ascending order
                        filteredEntities = queryParameters.SortOrder == SortOrder.Descending ?
                            filteredEntities.OrderByDescending(entity => entity.name.FirstName) :
                            filteredEntities.OrderBy(entity => entity.name.FirstName);
                        break;
                    case "lastname":
                        filteredEntities = queryParameters.SortOrder == SortOrder.Descending ?
                            filteredEntities.OrderByDescending(entity => entity.name.Surname) :
                            filteredEntities.OrderBy(entity => entity.name.Surname);
                        break;
                }
            }

            // Pagination code
            var pageNumber = queryParameters.PageNumber ?? 1; //default val is 1
            var pageSize = queryParameters.PageSize ?? entity_count; //default val is entity_count

            //return after applying pagination
            return Task.FromResult<IEnumerable<Entity>>(filteredEntities.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
        }

        // Simulates retrieving a single entity by ID from the database
        public async Task<Entity> GetEntityById(int id)
        {
            FailureSimulation("GET_BY_ID"); // Simulates database operation failure
            //look for entity with required ID and return it
            return (_entities.FirstOrDefault(entity => entity.id == id));
        }

        // Simulates creating a new entity in the database
        public async Task<Entity> CreateEntity(Entity entity)
        {

            FailureSimulation("CREATE"); // Simulates database operation failure
            entity.id = _entities.Max(e => e.id) + 1; // Generates a new unique ID
            _entities.Add(entity); // Adds the new entity
            return entity;
        }

        // Simulates updating an existing entity in the database
        public async Task<Entity> UpdateEntity(Entity entity)
        {
            FailureSimulation("UPDATE"); // Simulates database operation failure
            var existingEntity = _entities.FirstOrDefault(e => e.id == entity.id);
            if (existingEntity != null)
            {
                // Updates properties of existing entity with new values
                existingEntity.address = entity.address;
                existingEntity.date = entity.date;
                existingEntity.gender = entity.gender;
                existingEntity.name = entity.name;
            }
            return existingEntity;
        }

        // Simulates deleting an entity from the database
        public async Task<Entity> DeleteEntity(int id)
        {
            FailureSimulation("DELETE"); // Simulates database operation failure
            var entityToDelete = _entities.FirstOrDefault(e => e.id == id);
            if (entityToDelete != null)
            {
                _entities.Remove(entityToDelete); // Removes the entity
            }
            return (new Entity());
        }

        // Simulates database operation failure with a given probability
        private void FailureSimulation(string ops)
        {
            if (rnd.NextDouble() <= probability_failure && can_fail)
            {
                throw new Exception($"Database {ops} operation failed");
            }
        }
    }
}
