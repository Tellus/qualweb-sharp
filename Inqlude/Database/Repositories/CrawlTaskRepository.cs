using System;

namespace Inqlude.Database {
  public static class CrawlTaskRepository {
    static CrawlTaskRepository() {
      Console.WriteLine($"{nameof(CrawlTaskRepository)} instantiated.");
    }
  }
}