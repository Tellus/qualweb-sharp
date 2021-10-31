using MongoDB.Driver;
using System;

namespace Inqlude.Database {
  public static class Connection {

    private static MongoClient _client;
    public static MongoClient client {
      get {
        if (_client == null)
          throw new NullReferenceException($"MongoClient has not been initialized. Call {nameof(Connection.Configure)} before use.");
        else return _client;
      }

      private set {
        _client = value;
      }
    }

    public static string DefaultDatabase = "smEvaluations_sharp";

    public static readonly string DebugConnectionString = "mongodb://archtabby:NotARealCat@192.168.0.12:27017/smEvaluations_sharp?authSource=admin";

    public static IMongoDatabase database {
      get => Connection.client.GetDatabase(DefaultDatabase);
    }

    public static void Configure(MongoClientSettings settings) {
      Connection.client = new MongoClient(settings);
    }

    public static void Configure(string connectionString) {
      Connection.client = new MongoClient(connectionString);
    }

    public static void Configure(MongoUrl mongoUrl) {
      Connection.client = new MongoClient(mongoUrl);
    }
  }
}