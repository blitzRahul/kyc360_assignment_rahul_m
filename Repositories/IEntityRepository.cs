using kyc360_assignment_rahul_m.Models;

namespace kyc360_assignment_rahul_m.Repositories
{
    
    //contains definitions for what an instance of type IEntityRepository must contain
    //used by the controller (an instance of MockEntityRepository is provided to it via dependency injection)
    //MockEntityRepository implements this interface
        public interface IEntityRepository
        {
        
            //GetEntities on the basis of parameters
            Task<IEnumerable<Entity>> GetEntities(EntityQueryParameters queryParameters);
            //Get a single entity by ID
            Task<Entity> GetEntityById(int id);
        //create an entity
        Task<Entity> CreateEntity(Entity entity);
        //update an entity
        Task<Entity> UpdateEntity(Entity entity);
        //delete an entity
        Task<Entity> DeleteEntity(int id);

        }
    
}
