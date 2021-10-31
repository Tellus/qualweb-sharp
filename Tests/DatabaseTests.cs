using System;
using Inqlude.Database;
using Xunit;
using MongoDB.Driver.Core;
using MongoDB.Driver;

namespace InqludeTests.Database
{
  public class DatabaseTests : BaseDatabaseTest
  {
    [Fact]
    public async void DatabaseNamesTest()
    {
      ConfigureDefaults();

      var names = Connection.client.ListDatabaseNames();

      await names.ForEachAsync((string name) => Assert.NotEmpty(name));
    }
  }
}
