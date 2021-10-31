using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Qualweb {
  public enum EvaluationTaskState
  {
    Created,
    Running,
    Failed,
    Done,
  }

  public class EvaluationTask {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }

    [BsonElement("crawlUrl")]
    public string CrawlUrl { get; set; }

    [BsonExtraElements]
    public BsonDocument surplus;
  }
}