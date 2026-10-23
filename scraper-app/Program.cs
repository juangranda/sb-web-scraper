using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using scraper_app.Application;
using scraper_app.CrawlServices;
using scraper_app.Filters;
using scraper_app.Models;
using scraper_app.Storage;
using scraper_app.TextServices;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

//const string Url = "https://news.ycombinator.com/";
//const int MaxEntries = 30;

var services = new ServiceCollection()
	.AddSingleton(_ =>
	{
		var http = new HttpClient();
		http.DefaultRequestHeaders.UserAgent.ParseAdd("HackerNewsScraper/1.0");
		return http;
	})
	.AddSingleton<IWordCounter, WordCounter>()
	.AddSingleton<IPageFetcher, HttpPageFetcher>()
	.AddSingleton<IEntryParser, NewsParser>()
	.AddSingleton<INewsCrawler, NewsCrawler>()
	.AddSingleton<IEntryFilter, NoFilter>()
	.AddSingleton<IEntryFilter, LongTitleFilter>()
	.AddSingleton<IEntryFilter, ShortTitleFilter>()
	.AddSingleton<IFilterCatalog, FilterCatalog>()
	.AddSingleton<IUsageStore>(_ => new UsageStore(Path.Combine(Directory.GetCurrentDirectory(), "data", "usage.db")))
	.AddSingleton<ScraperApplication>();

await using var provider = services.BuildServiceProvider();
var app = provider.GetRequiredService<ScraperApplication>().RunAsync(args);
return await app;

// // var filters = new Dictionary<string, (string Heading, Func<IEnumerable<Entry>, IEnumerable<Entry>> Apply)>
// // {
// // 	["none"] = ("All entries", e => e),
// // 	["long-titles"] = ("More than five words in the title, by comments (desc)",
// // 		e => e.Where(x => CountWords(x.Title) > 5).OrderByDescending(x => x.Comments)),
// // 	["short-titles"] = ("Five or fewer words in the title, by points (desc)",
// // 		e => e.Where(x => CountWords(x.Title) <= 5).OrderByDescending(x => x.Points)),
// // };

// // var directory = Path.Combine(Directory.GetCurrentDirectory(), "data", "usage.db");

// // var usageStore = new UsageStore(directory);

// // var filterName = args.FirstOrDefault() ?? NoFilter.FilterName;

// // if (filterName == "history")
// // {
// // 	usageStore.PrintRecent(10);
// // 	return;
// // }

// // var record = new ScrapeRecord
// // {
// // 	TimestampUtc = DateTime.UtcNow,
// // 	Filter = filterName,
// // };

// // var total = Stopwatch.StartNew();
// // try
// // {
// // 	if (filters.TryGetValue(filterName, out var filter))
// // 	{
// // 		using var http = new HttpClient();
// // 		http.DefaultRequestHeaders.UserAgent.ParseAdd("HackerNewsScraper/1.0");

// // 		var fetch = Stopwatch.StartNew();
// // 		using var response = await http.GetAsync(Url);
// // 		record.HttpStatus = (int)response.StatusCode;
// // 		response.EnsureSuccessStatusCode();
// // 		var html = await response.Content.ReadAsStringAsync();
// // 		record.FetchMs = fetch.ElapsedMilliseconds;
// // 		record.ResponseBytes = Encoding.UTF8.GetByteCount(html);

// // 		var entries = Scrape(html);
// // 		var result = filter.Apply(entries).ToList();
// // 		record.EntriesScraped = entries.Count;
// // 		record.EntriesReturned = result.Count;

// // 		PrintTable(filter.Heading, result);
// // 		record.Success = true;
// // 	}
// // 	else
// // 	{
// // 		throw new ArgumentException($"Unknown filter '{filterName}'. Use one of: {string.Join(", ", filters.Keys)}.");
// // 	}
// // }
// // catch (Exception ex)
// // {
// // 	record.Success = false;
// // 	record.Error = ex.Message;
// // 	Console.Error.WriteLine($"Error: {ex.Message}");
// // }
// // finally
// // {
// // 	record.DurationMs = total.ElapsedMilliseconds;
// // 	usageStore.Save(record);
// // }

// // static List<Entry> Scrape(string html)
// // {
// // 	var doc = new HtmlDocument();
// // 	doc.LoadHtml(html);

// // 	var entries = new List<Entry>();
// // 	var rows = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'athing')]") ?? Enumerable.Empty<HtmlNode>();

// // 	foreach (var row in rows.Take(MaxEntries))
// // 	{
// // 		var number = ParseInt(row.SelectSingleNode(".//span[@class='rank']")?.InnerText);
// // 		var title = HtmlEntity.DeEntitize(
// // 			row.SelectSingleNode(".//span[@class='titleline']/a")?.InnerText ?? "").Trim();

// // 		// Points and comments live in the following <tr> (td.subtext).
// // 		var subtext = row.SelectSingleNode("following-sibling::tr[1]//td[@class='subtext']");
// // 		var points = ParseInt(subtext?.SelectSingleNode(".//span[@class='score']")?.InnerText);
// // 		var commentsLink = subtext?.SelectNodes(".//a")
// // 			?.LastOrDefault(a => HtmlEntity.DeEntitize(a.InnerText).Contains("comment"));
// // 		var comments = ParseInt(commentsLink?.InnerText);

// // 		entries.Add(new Entry(number, title, points, comments));
// // 	}
// // 	return entries;
// // }

// // static void PrintTable(string heading, IEnumerable<Entry> items)
// // {
// // 	Console.WriteLine();
// // 	Console.WriteLine(heading);
// // 	Console.WriteLine($"{"#",3}  {"Points",6}  {"Comments",8}  {"Words",5}  Title");
// // 	Console.WriteLine(new string('-', 80));
// // 	foreach (var e in items)
// // 	{
// // 		Console.WriteLine($"{e.Number,3}  {e.Points,6}  {e.Comments,8}  {CountWords(e.Title),5}  {e.Title}");
// // 	}
// // }

// // static int CountWords(string text)
// // {
// // 	var textSplit = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
// // 	var count =	textSplit.Count(token => token.Any(char.IsLetterOrDigit));
// // 	return count;
// // }

// // // Extracts the first integer in the text ("123 points", "45&nbsp;comments", "7."); 0 if none.
// // static int ParseInt(string? text)
// // {
// // 	if (string.IsNullOrEmpty(text)) return 0;
// // 	var match = Regex.Match(text, @"\d+");
// // 	return match.Success ? int.Parse(match.Value) : 0;
// // }