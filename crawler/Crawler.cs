using System;
using Microsoft.Playwright;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Inqlude;

namespace Qualweb {
  public record CrawlOptions {
    /// <summary>
    /// How many links from the origin point should the crawler go? This
    /// controls how many navigations a page can be from the source before the
    /// crawler stops.
    /// </summary>
    public int maxLinkDepth = int.MaxValue;
    /// <summary>
    /// How many path segments should be allowed? Assume a source URL 
    /// "https://somedomain.null/seg1" and a max path depth of 2. The source
    /// URL has a depth of 1 ("/seg1"), so the URL
    /// "https://somedomain.null/seg1/seg2" will be included by the URL
    /// "https://somedomain.null/seg1/seg2/seg3" will <b>not</b>.
    /// </summary>
    public int maxPathDepth = int.MaxValue;

    /// <summary>
    /// Stop crawling after this many URLs have been found.
    /// </summary>
    public int maxUrls = int.MaxValue;

    /// <summary>
    /// Optional timeout.
    /// </summary>
    public int timeout = 10000;

    /// <summary>
    /// If set (and greater than 1), the crawler will attempt to run several
    /// active trawl processes in parallel. If the target web server uses some
    /// sort of throttling, you may see performance degradation from using
    /// this option.
    /// </summary>
    public int maxParallelCrawls = 1;

    /// <summary>
    /// If set, will use this viewport for pages before trawling them.
    /// </summary>
    public ViewportSize viewportSize;
  }

  /// <summary>
  /// Valid states for an item in the queue.
  /// </summary>

  public class QueueItem<T> {
    public T Data;

    public QueueState State;

    public string url {
      get;
      set;
    }
  }

  /// <summary>
  /// Generic queue interface compatible with the <see cref="Crawler"/>. The
  /// crawler assumes that keys (URLs) are unique throughout the queue.
  /// </summary>
  /// <typeparam name="ValueT"></typeparam>
  /// <typeparam name="KeyT"></typeparam>
  public interface ICrawlQueue {
    /// <summary>
    /// Check whether an item is somewhere in the queue.
    /// </summary>
    /// <param name="item">The item to look for.</param>
    /// <returns>True if the item is in the queue. This does <b>not</b> mean
    /// that the item has not yet been processed.</returns>
    bool Has(QueueItem<string> item);
    bool Has(string url);

    /// <summary>
    /// Retrieve the item as given by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    QueueItem<string> Get(string url);

    void Add(string url);
    void Add(string url, QueueState state);

    /// <summary>
    /// Checks whether the queue currently has any items awaiting processing.
    /// </summary>
    /// <returns>True if at least one item can be dequeued.</returns>
    bool HasNext();

    /// <summary>
    /// Checks whether the queue currently has any items awaiting processing or
    /// are currently being processed.
    /// </summary>
    /// <returns>True if at least one item is Queued or Running.</returns>
    bool HasNextOrRunning();

    /// <summary>
    /// Retrieves the next <see cref="QueueItem"/> that is queued up. Throws
    /// an error if no queued items remain.
    /// </summary>
    /// <returns></returns>
    QueueItem<string> GetNext();

    void MarkDownloaded(string url);

    IEnumerable<QueueItem<string>> GetAll();
  }

  /// <summary>
  /// The memory queue is the crawler's default queue mechanism. It keeps all
  /// URLs in local memory. For small crawl tasks, this is fine, but for large
  /// set of URLs, or where data distribution is important, you may need to
  /// implement your own ICrawlQueue.
  /// </summary>
  public class MemoryQueue : ICrawlQueue {
    readonly private ConcurrentDictionary<string, QueueItem<string>> queue;

    /// <summary>
    /// Creates a new MemoryQueue backed by a <seealso cref="ConcurrentDictionary"/>.
    /// </summary>
    /// <param name="parallelCount">Estimated number of threads that will operate on the queue.</param>
    /// <param name="initialCapacity">Initial capacity of the underlying
    /// ConcurrentDictionary. The dictionary will resize as needed, but higher
    /// concurrencies make this operation more expensive.</param>
    public MemoryQueue(int parallelCount = 1, int initialCapacity = 100) {
      this.queue = new ConcurrentDictionary<string, QueueItem<string>>(parallelCount, initialCapacity);
    }

    public bool Has(string url) {
      return this.queue.ContainsKey(url);
    }

    public bool Has(QueueItem<string> item) {
      return this.queue.ContainsKey(item.url);
    }

    public QueueItem<string> Get(string url) {
      return this.queue[url];
    }

    public void Add(string url) {
      this.Add(url, QueueState.Queued);
    }

    public void Add(string url, QueueState state = QueueState.Queued) {
      this.queue.TryAdd(url, new QueueItem<string>() {
        Data = url,
        State = state,
        url = url,
      });
    }

    public void MarkDownloaded(string url) {
      this.queue.AddOrUpdate(
        url,
        u => throw new Exception("Should not happen!"),
        (u, queueItem) => {
          queueItem.State = QueueState.EvaluationTaskSpawned;
          return queueItem;
        }
      );
    }

    public bool HasNext() {
      return this.queue.Any(item => item.Value.State == QueueState.Queued);
    }

    public bool HasNextOrRunning() {
      return this.queue.Any(item => item.Value.State == QueueState.Queued || item.Value.State == QueueState.Headers || item.Value.State == QueueState.Spooled);
    }

    public QueueItem<string> GetNext() {
      var nextItem = this.queue.First(item => item.Value.State == QueueState.Queued);

      nextItem.Value.State = QueueState.Spooled;

      return nextItem.Value;
    }

    public IEnumerable<QueueItem<string>> GetAll() {
      return this.queue.Values;
    }
  }

  /// <summary>
  /// Link-seeking crawler. The crawler seeks out the pages of a website,
  /// cataloguing the unique and valid URLs that it encounters. The crawler
  /// itself does <b>not</b> log any other data, such as serializing the DOM.
  /// </summary>
  public class Crawler {
    /// <summary>
    /// Browser instance used for crawling.
    /// </summary>
    IBrowser browser {
      get; init;
    }

    /// <summary>
    /// Raised <b>after</b> a page has been loaded but <b>before</b> the crawler
    /// trawls the page for links.
    /// </summary>
    public event EventHandler<IPage> Loaded;

    ICrawlQueue queue;

    /// <summary>
    /// Raised every time a valid URL is discovered. If the same URL is
    /// encountered multiple times, this event will be raised for each one.
    /// </summary>
    public event EventHandler<string> UrlDiscovered;

    protected Crawler() {
    }

    public async Task<IEnumerable<QueueItem<string>>> crawl(string baseUrl, CrawlOptions crawlOptions) {
      this.queue = new MemoryQueue(crawlOptions.maxParallelCrawls);

      this.queue.Add(baseUrl);

      // Initial URL is always handled individually before we try to add
      // parallel handling.
      await this.fetchPageLinks(baseUrl);

      List<Task> taskList = new List<Task>(crawlOptions.maxParallelCrawls);

      for (int i = 0; i < crawlOptions.maxParallelCrawls; i++) {
        var localThreadId = i;
        taskList.Add(Task.Run(async () => {
          while (this.queue.HasNextOrRunning()) {
            var next = this.queue.GetNext();

            if (next != null) {
              Console.WriteLine($"Thread #{localThreadId}: {next.url}");
              await this.fetchPageLinks(next.url);
            }
          }
        }));
      }

      await Task.WhenAll(taskList);

      return this.queue.GetAll();
    }

    /// <summary>Navigates to <paramref name="url"/>, discovers its links, and recursively
    /// trawls each of them until the max link depth has been reached, or no
    /// new links have been discovered. </summary>
    /// <param name="url">The URL to navigate to and trawl.</param>
    protected async Task fetchPageLinks(string url) {
      Console.WriteLine($"Fetching links for {url}");

      Uri uri = new Uri(url);

      var page = await this.browser.NewPageAsync(new BrowserNewPageOptions {
        
      });

      // TODO: viewport change.

      await page.GotoAsync(url, new PageGotoOptions {
        WaitUntil = WaitUntilState.Load,
      });

      this.Loaded?.Invoke(this, page);

      // Get all link elements in the DOM, then request their href attribute.
      var linkElements = (await page.QuerySelectorAllAsync("body a"))
        .Select(el => el.GetAttributeAsync("href"));

      // Wait for alll of these tasks to complete.
      await Task.WhenAll(linkElements);

      // Select the result (the href) and only keep the ones that aren't null.
      var hrefs = linkElements.Where(tsk => tsk.Result != null)
                              .Select(tsk => tsk.Result);

      var validLinkStarts = new List<string>() { "http", "#" };
      var invalidLinkContents = new List<string>() { "javascript:", "mailto:", "tel:" };
      var invalidFileExts = new List<string>() {
        "png",
        "gif",
        "jpg",
        "pdf",
        "jpeg",
        "svg",
        "docx",
        "js",
        "ico",
        "xml",
        "mp4",
        "mp3",
        "mkv",
        "wav", 
        "rss",
        "json",
        "pptx",
        "txt",
      };

      foreach (var href in hrefs) {
        // If there is no href, if it has an uninteresting file extension, or
        // if it contains suggestions that it contains javascript or mail
        // links, ignore it.
        if (href == null) {
          Console.WriteLine("ERROR! Null href was passed! This should not happen!");
        } else if (this.queue.Has(href)) continue;
        else if (
          invalidFileExts.Any(end => href.EndsWith(end)) ||
          invalidLinkContents.Any(content => href.Contains(content)) ||
          href.StartsWith("#")
        ) {
          this.queue.Add(href, QueueState.InvalidDomain);
        } else if (href.StartsWith("/")) {
          // Absolute path. Concatenate with the base.
          this.queue.Add(new Uri(uri, href).AbsoluteUri);
        } else if (href.StartsWith("./")) {
          // Weird relative path. Chop off the period and append to the base.
          this.queue.Add(new Uri(uri, href.Substring(1)).AbsoluteUri);
        } else if (href.StartsWith("http")) {
          // Full URL. Make sure it's a whitelisted domain (i.e. the same
          // domain)

          // TODO: Add custom whitelisting here!
          var linkedUri = new Uri(href);
          if (linkedUri.Host == uri.Host) {
            this.queue.Add(linkedUri.OriginalString);
          }
        } else {
          Console.WriteLine($"Unknown/unsupported URL { href }.");
        }
      }

      this.queue.MarkDownloaded(url);

      await page.CloseAsync(new PageCloseOptions { RunBeforeUnload = false });      
    }

    public static async Task<Crawler> createCrawlerAsync() {

      var playwright = await Playwright.CreateAsync();

      var crawler = new Crawler() {
        browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
          // Headless = false
        })
      };

      return crawler;
    }
  }
}