using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Qualweb
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
      };

      crawlCommand.Handler = CommandHandler.Create<string, int, int>(RunCrawler);

      return new RootCommand("Qualweb is a web accessibility evaluation tool. This version is written in C#.") {
        crawlCommand,
      };
    }

    static async Task RunCrawler(string baseUrl, int maxLinkDepth = 1, int maxPathDepth = 1) {
      Console.WriteLine($"Running Crawler from {baseUrl}. Path depth: { maxPathDepth }, link depth: { maxLinkDepth }.");

      var crawler = await Crawler.createCrawlerAsync();

      var links = await crawler.crawl(baseUrl, new CrawlOptions {
        maxLinkDepth = 2,
      });

      Console.WriteLine("Crawler discovered the following links:");

      foreach (var l in links)
        Console.WriteLine($"\t{l}");
    }
  }
}
