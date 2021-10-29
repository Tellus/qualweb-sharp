using MongoDB.Driver;

namespace Inqlude {
  public static class Connection {
    public static readonly MongoClient client;

    public static IMongoDatabase database {
      get => Connection.client.GetDatabase("smEvaluations");
    }

    static Connection() {
      Connection.client = new MongoClient("mongodb://archtabby:NotARealCat@192.168.0.12:27017/smEvaluations?authSource=admin");
    }
  }
}