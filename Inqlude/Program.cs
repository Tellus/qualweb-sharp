using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Microsoft.Playwright;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Qualweb;
using NLog;
using NLog.Extensions.Logging;

namespace Inqlude.Database
{
  class Program
  {
    static Logger logger {
      get => LogManager.GetCurrentClassLogger();
    }

    public static async Task Main(params string[] args) {
      var cmd = BuildCommand();

      // var loggerConfiguration = new NLog.Config.LoggingConfiguration();

      // var logToFile = new NLog.Targets.FileTarget("logfile") {
      //   FileName = System.IO.Path.Join("log", "example.log"),
      // };

      // var logToConsole = new NLog.Targets.ConsoleTarget("logconsole");

      // loggerConfiguration.AddRule(LogLevel.Trace, LogLevel.Fatal, logToFile);
      // loggerConfiguration.AddRule(LogLevel.Trace, LogLevel.Fatal, logToConsole);

      // LogManager.Configuration = loggerConfiguration;

      var defaultInternalLogLevel = NLog.Common.InternalLogger.LogLevel;
      NLog.Common.InternalLogger.LogToConsole = true;
      NLog.Common.InternalLogger.LogLevel = LogLevel.Error;

      EventHandler<NLog.Common.InternalLoggerMessageEventArgs> configLoadEventHandler = (object source, NLog.Common.InternalLoggerMessageEventArgs args) => {
        if (args.Level == LogLevel.Error) {
          Console.WriteLine("Something bad happened while parsing the logging config file. Program will now exit.");
          System.Diagnostics.Process.GetCurrentProcess().Kill(true);
        }
      };

      NLog.Common.InternalLogger.LogMessageReceived += configLoadEventHandler;

      LogManager.LoadConfiguration("nlog.config");

      // If we reach this code, no error happened while loading the config.
      // Remove the event handler again.
      NLog.Common.InternalLogger.LogMessageReceived += configLoadEventHandler;

      NLog.Common.InternalLogger.LogToConsole = false;
      NLog.Common.InternalLogger.LogLevel = defaultInternalLogLevel;

      logger.Debug("NLog loaded. Continuing with command invocation.");

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
      logger.Info($"Running Crawler from {baseUrl}. Path depth: { maxPathDepth }, link depth: { maxLinkDepth }.");

      var crawler = await Crawler.createCrawlerAsync();

      var links = await crawler.crawl(baseUrl, new CrawlOptions {
        maxLinkDepth = maxLinkDepth,
        maxParallelCrawls = maxParallel,
      });

      logger.Info("Crawler discovered the following links:");

      foreach (var l in links.Where(l => l.State == QueueState.EvaluationTaskSpawned))
        logger.Info($"\t{l.url}");
    }

    static async Task RunDatabase() {
      logger.Info("Getting next queued item");

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
