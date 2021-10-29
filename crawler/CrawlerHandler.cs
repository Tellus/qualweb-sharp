using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System;

namespace Inqlude {
  public class CrawlerHandler {

    private static FilterDefinitionBuilder<CrawlQueueItem> filterBuilder = new FilterDefinitionBuilder<CrawlQueueItem>();
    private static UpdateDefinitionBuilder<CrawlQueueItem> updateBuilder = new UpdateDefinitionBuilder<CrawlQueueItem>();

    public static FilterDefinition<CrawlQueueItem> NextQueuedItemFilter() =>
      filterBuilder.Eq<string>("Status", "queued");

    public async static Task<CrawlQueueItem> GetAndMarkNext() {
      var col = Connection.database.GetCollection<CrawlQueueItem>("crawltaskurls");

      try {
        return await col.FindOneAndUpdateAsync(
          NextQueuedItemFilter(),
          updateBuilder.Set("status", "spooled"),
          new FindOneAndUpdateOptions<CrawlQueueItem> {
            ReturnDocument = ReturnDocument.After
          }
        );
      } catch (Exception e) {
        Console.WriteLine("Welp, something f'ed up during the query.");
        Console.WriteLine(e.Message);
        return null;
      }
    }
  }
}