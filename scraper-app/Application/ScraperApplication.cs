using System.Diagnostics;
using scraper_app.CrawlServices;
using scraper_app.Filters;
using scraper_app.Models;
using scraper_app.PrintServices;
using scraper_app.Storage;

namespace scraper_app.Application;

public class ScraperApplication(
	INewsCrawler newsCrawler,
	IUsageStore usageStore,
	IFilterCatalog filters,
	IConsolePrinter printer)
{
	private const string Url = "https://news.ycombinator.com/";
	private const int MaxEntries = 30;
	private const string HistoryCommand = "history";

	public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
	{
		var filterName = args.FirstOrDefault() ?? NoFilter.FilterName;

		if (filterName == HistoryCommand)
		{
			var history = await usageStore.GetRecentAsync(20, cancellationToken);
			printer.PrintHistory(history);
			return 0;
		}

		return await RunFilterAsync(filterName, cancellationToken);
	}

	private async Task<int> RunFilterAsync(string filterName, CancellationToken cancellationToken)
	{
		var record = new ScrapeRecord
		{
			TimestampUtc = DateTime.UtcNow,
			Filter = filterName,
		};

		var total = Stopwatch.StartNew();
		try
		{
			if (filters.TryGetFilter(filterName, out var filter))
			{
				var result = await newsCrawler.CrawlAsync(Url, MaxEntries, cancellationToken);
				record.HttpStatus = result.Page.StatusCode;
				record.FetchMs = (long)result.Page.Elapsed.TotalMilliseconds;
				record.ResponseBytes = result.Page.ResponseBytes;
				record.EntriesScraped = result.Entries.Count;

				var selected = filter.Apply(result.Entries).ToList();
				record.EntriesReturned = selected.Count;
				
				printer.PrintEntries(filter.Name, selected);

				record.Success = true;
			}
			else
			{
				throw new ArgumentException($"Unknown filter '{filterName}'. Use one of: {string.Join(", ", filters.Names)}, {HistoryCommand}.");
			}
		}
		catch (Exception ex)
		{
			record.Success = false;
			record.Error = ex.Message;
			printer.PrintError(ex.Message);
		}
		finally
		{
			record.DurationMs = total.ElapsedMilliseconds;
			await usageStore.SaveAsync(record, cancellationToken);
		}

		return record.Success ? 0 : 1;
	}
}