using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Microsoft.Playwright;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Qualweb;

namespace Inqlude
{
  class Program
  {
    public static async Task Main(params string[] args) {
      var cmd = BuildCommand();

      await cmd.InvokeAsync(args);

      return;

      // Console.WriteLine("Hello World!");

      // using var playwright = await Playwright.CreateAsync();

      // await using var browser = await playwright.Chromium.LaunchAsync();

      // var page = await browser.NewPageAsync();

      // await page.GotoAsync("https://www.inklusio.dk");

      // await page.ScreenshotAsync(new PageScreenshotOptions { Path = "screenshot.png" });

      // var result = await page.GetAttributeAsync("html", "lang", new PageGetAttributeOptions { Strict = true });

      // Console.WriteLine($"Page has language \"{ result }\"");
    }

    static RootCommand BuildCommand() {
      var crawlCommand = new Command("crawl", "Invokes the crawler.") {
        new Argument("baseUrl") {
          Description = "The URL to start crawling from.",
        },
        new Option<int>(
          "--max-link-depth",
          "The crawler will not crawl pages that are more than this many LINKS deep. See also --max-path-depth."
        ),
        new Option<int>(
          "--max-path-depth",
          "The crawler will not crawl pages that have this many segments in the URL's path. See also --max-link-depth."
        ),
        new Option<int>(
          "--max-parallel",
          "How many parallel tasks to use while crawling."
        )
      };

      crawlCommand.Handler = CommandHandler.Create<string, int, int, int>(RunCrawler);

      var dbCommand = new Command("db", "Messes around with the database") {
        Handler = CommandHandler.Create(RunDatabase),
      };


      return new RootCommand("Qualweb is a web accessibility evaluation tool. This version is written in C#.") {
        crawlCommand,
        dbCommand,
      };
    }

    static async Task RunCrawler(string baseUrl, int maxLinkDepth = 1, int maxPathDepth = 1, int maxParallel = 1) {
      Console.WriteLine($"Running Crawler from {baseUrl}. Path depth: { maxPathDepth }, link depth: { maxLinkDepth }.");

      var crawler = await Crawler.createCrawlerAsync();

      var links = await crawler.crawl(baseUrl, new CrawlOptions {
        maxLinkDepth = maxLinkDepth,
        maxParallelCrawls = maxParallel,
      });

      Console.WriteLine("Crawler discovered the following links:");

      foreach (var l in links.Where(l => l.State == QueueState.EvaluationTaskSpawned))
        Console.WriteLine($"\t{l.url}");
    }

    static async Task RunDatabase() {
      Console.WriteLine("Getting next queued item");

      // var next = await CrawlerHandler.GetAndMarkNext();

      // Console.WriteLine($"{next.Id}: {next.Url}");

      // Console.WriteLine("Connecting to database.");

      // var client = new Inqlude.Connection();

      // var dbNames = await client.client.ListDatabaseNamesAsync();

      // dbNames.MoveNext();
      // foreach (var name in dbNames.Current) {
      //   Console.WriteLine(name);
      // }

      // Console.WriteLine("Wooohooo?");

      // var collection = client.client.GetDatabase("smEvaluations").GetCollection<EvaluationTask>("evaluationtasks");

      // var builder = new FilterDefinitionBuilder<EvaluationTask>();

      // var evalTask = collection.Find(builder.Regex("crawlUrl", new BsonRegularExpression("/minds\\.dk/"))).First();

      // Console.WriteLine($"{evalTask._id}: {evalTask.CrawlUrl}");
    }
  }
}
