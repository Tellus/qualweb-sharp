using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;

namespace Inqlude.Database {
  public abstract class BaseRepository<ModelT> {
    protected readonly IMongoCollection<ModelT> collection;

    public readonly FilterDefinitionBuilder<ModelT> Builder = new FilterDefinitionBuilder<ModelT>();

    public BaseRepository(string collectionName, MongoCollectionSettings collectionSettings = null) {
      this.collection = Connection.database.GetCollection<ModelT>(collectionName, collectionSettings);
    }

    public async Task<ModelT> GetById(string objectId) {
      return await collection.Find(Builder.Eq("Id", objectId)).FirstAsync();
    }

    public async Task<ModelT> CreateOne(ModelT newTask) {
      await collection.InsertOneAsync(newTask);

      return newTask;
    }

    /// <summary>
    /// A simple variant of Find that takes an object, and uses the values of
    /// its fields in an And/Equality search. This is good for basic searches
    /// like "all UrlEvaluationTasks with this specific EvaluationTask" or
    /// "all EvaluationTasks using this specific Qualweb version".
    /// </summary>
    /// <param name="newTask"></param>
    /// <returns></returns>
    public async Task<IAsyncCursor<ModelT>> Find(ModelT newTask, FindOptions<ModelT> options = null) {
      var type = newTask.GetType();

      var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

      return await collection.FindAsync(Builder.And(
        fields.Where(f => f != null)
              .Select(f => Builder.Eq(f.Name, f.GetValue(newTask)))
        ),
        options
      );
    }

    /// <summary>
    /// Regular FindMany query. Proxies down to the collection-level find
    /// methods.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task<IAsyncCursor<ModelT>> FindAsync(FilterDefinition<ModelT> filter, FindOptions<ModelT> options = null) {
      return await collection.FindAsync<ModelT>(filter, options);
    }

    public async Task<ModelT> FindOneAsync(FilterDefinition<ModelT> filter, FindOptions options = null) {
      return await collection.Find<ModelT>(filter, options).FirstAsync();
    }

    /// <summary>
    /// Estimates the number of documents in the collection. If <paramref name="filter"/>
    /// is passed, MongoDB will probably try to use the index to speed up an
    /// accurate count. Using a filter without indexed fields will probably lead
    /// to a full collection scan.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<long> Count(FilterDefinition<ModelT> filter = null) {
      if (filter == null) return collection.EstimatedDocumentCount();
      else return await collection.CountDocumentsAsync(filter);
    }
  }
}