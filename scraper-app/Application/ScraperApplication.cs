using System.Diagnostics;
using scraper_app.CrawlServices;
using scraper_app.Filters;
using scraper_app.Models;
using scraper_app.Storage;

namespace scraper_app.Application;

public class ScraperApplication(
	INewsCrawler newsCrawler,
	IUsageStore usageStore)
{
	public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
	{
		var filterName = args.FirstOrDefault() ?? NoFilter.FilterName;

		if (filterName == "history")
		{
			usageStore.PrintRecent(10);
			return 0;
		}

		return await RunFilterAsync(filterName, cancellationToken);
	}

	private async Task<int> RunFilterAsync(string filterName, CancellationToken cancellationToken)
	{
		//TODO: wire up filters from DI instead of hardcoding them here
		var filters = new Dictionary<string, (string Heading, Func<IEnumerable<Entry>, IEnumerable<Entry>> Apply)>
		{
			["none"] = ("All entries", e => e),
			["long-titles"] = ("More than five words in the title, by comments (desc)",
								e => e.Where(x => CountWords(x.Title) > 5).OrderByDescending(x => x.Comments)),
			["short-titles"] = ("Five or fewer words in the title, by points (desc)",
								e => e.Where(x => CountWords(x.Title) <= 5).OrderByDescending(x => x.Points))
		};

		var record = new ScrapeRecord
		{
			TimestampUtc = DateTime.UtcNow,
			Filter = filterName,
		};

		var total = Stopwatch.StartNew();
		try
		{
			if (filters.TryGetValue(filterName, out var filter))
			{
				var result = await newsCrawler.CrawlAsync("https://news.ycombinator.com/", 30, cancellationToken);
				record.HttpStatus = result.Page.StatusCode;
				record.FetchMs = (long)result.Page.Elapsed.TotalMilliseconds;
				record.ResponseBytes = result.Page.ResponseBytes;
				record.EntriesScraped = result.Entries.Count;

				var selected = filter.Apply(result.Entries).ToList();
				record.EntriesReturned = selected.Count;
				
				PrintTable(filter.Heading, selected);

				record.Success = true;
			}
			else
			{
				throw new ArgumentException($"Unknown filter '{filterName}'. Use one of: {string.Join(", ", filters.Keys)}.");
			}
		}
		catch (Exception ex)
		{
			record.Success = false;
			record.Error = ex.Message;
			Console.Error.WriteLine($"Error: {ex.Message}");
		}
		finally
		{
			record.DurationMs = total.ElapsedMilliseconds;
			usageStore.Save(record);
		}

		return record.Success ? 0 : 1;
	}

	private static int CountWords(string text)
	{
		var textSplit = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
		var count =	textSplit.Count(token => token.Any(char.IsLetterOrDigit));
		return count;
	}

	//TODO: move to a separate service class and wire up via DI
	static void PrintTable(string heading, IEnumerable<Entry> items)
	{
		Console.WriteLine();
		Console.WriteLine(heading);
		Console.WriteLine($"{"#",3}  {"Points",6}  {"Comments",8}  {"Words",5}  Title");
		Console.WriteLine(new string('-', 80));
		foreach (var e in items)
		{
			Console.WriteLine($"{e.Number,3}  {e.Points,6}  {e.Comments,8}  {CountWords(e.Title),5}  {e.Title}");
		}
	}
}