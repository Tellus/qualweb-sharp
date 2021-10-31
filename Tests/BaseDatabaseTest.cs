using Inqlude.Database;

namespace InqludeTests.Database {
  /// <summary>
  /// An abstract base for database-specific tests.
  /// </summary>
  public abstract class BaseDatabaseTest {
    protected void ConfigureDefaults() {
      Connection.Configure(Connection.DebugConnectionString);
    }
  }
}