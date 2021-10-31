using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Inqlude.Database {
  public enum EvaluationTaskState {
    Created,
    Running,
    Completed,
    Failed,
  };

  public class EvaluationTask {
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("customerCase")]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string CustomerCase { get; set; }

    [BsonElement("currentState")]
    public EvaluationTaskState State { get; set; }

    [BsonElement("creationDate")]
    [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]
    public DateTime CreationDate { get; init; }

    [BsonElement("completionDate")]
    [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]
    public DateTime CompletionDate { get; init; }

    [BsonElement("failedUrls")]
    [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]
    public uint FailedUrls { get; set; }

    [BsonElement("completedUrls")]
    [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]
    public uint CompletedUrls { get; set; }

    [BsonElement("crawlUrl")]
    public string CrawlUrl { get; set; }

    [BsonElement("modules")]
    public ICollection<string> QualwebModules { get; set; }

    [BsonElement("levels")]
    public ICollection<string> ConformanceLevels { get; set; }

    [BsonElement("qualwebVersion")]
    public string QualwebVersion { get; set; }
  }

  public class EvaluationTaskRepository : BaseRepository<EvaluationTask> {
    public EvaluationTaskRepository() : base("evaluationtasks") {
      
    }

    public EvaluationTask GetOne() {
      return collection.Find(FilterDefinition<EvaluationTask>.Empty).First();
    }
  }
}