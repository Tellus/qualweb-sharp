using Inqlude.Database;
using Inqlude;
using Xunit;
using System;

namespace InqludeTests.Database {
  public class ConnectionTests {
    [Fact]
    public void GoodConnection() {
      Connection.Configure(Connection.DebugConnectionString);

      Connection.database.ListCollectionNames();
    }

    [Fact]
    public void BadConnection() {
      Assert.Throws<NullReferenceException>(() => {
        Connection.client.ListDatabaseNames();
      });
    }
  }
}