using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Inqlude {
  public enum QueueState {
    /// <summary>
    /// Newly-created, but NOT ready to be picked up by the Crawler yet. Use
    /// this as a staging state if necessary.
    /// </summary>
    Created,

    /// <summary>
    /// Ready for processing by a Crawler.
    /// </summary>
    Queued,

    /// <summary>
    /// The item has been picked up by a Crawler and is about to be processed.
    /// </summary>
    Spooled,

    /// <summary>
    /// The Crawler has completed the HTTP(S) request to the URL. 
    /// </summary>
    Headers,

    /// <summary>
    /// An UrlEvaluationTask has been spawned for this item.
    /// </summary>
    EvaluationTaskSpawned,

    /// <summary>
    /// The URL is not on the CrawlTask's whitelist.
    /// </summary>
    InvalidDomain,

    /// <summary>
    /// The crawler failed to process the item for unknown/other reasons.
    /// </summary>
    Failed,
  }

  public class CrawlQueueItem {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id;

    [BsonElement("status")]
    public string Status { get; set; }

    [BsonElement("url")]
    public string Url { get; set; }

    [BsonElement("linkDepth")]
    public uint LinkDepth { get; set; }

    [BsonElement("crawlTask")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CrawlTask { get; set; }

    [BsonElement("contentType")]
    public string ContentType { get; set; }

    [BsonElement("creationDate")]
    public DateTime CreationDate { get; set; }

    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    [BsonElement("completionDate")]
    public DateTime CompletionDate { get; set; }

    [BsonElement("code")]
    public uint StatusCode { get; set; }

    [BsonElement("headers")]
    public BsonDocument headers{ get; set; }

    [BsonExtraElements]
    public BsonDocument extras;
  }
}