using System;

namespace Inqlude {
  public static class CrawlTaskRepository {
    static CrawlTaskRepository() {
      Console.WriteLine($"{nameof(CrawlTaskRepository)} instantiated.");
    }
  }
}