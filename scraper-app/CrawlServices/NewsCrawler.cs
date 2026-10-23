using scraper_app.Models;

namespace scraper_app.CrawlServices;

public interface INewsCrawler
{
	Task<CrawlResult> CrawlAsync(string url, int maxEntries, CancellationToken cancellationToken = default);
}

public class NewsCrawler(IPageFetcher fetcher, IEntryParser parser) : INewsCrawler
{
	public async Task<CrawlResult> CrawlAsync(string url, int maxEntries, CancellationToken cancellationToken = default)
	{
		var page = await fetcher.FetchAsync(url, cancellationToken);
		var entries = parser.Parse(page.Html, maxEntries);
		return new CrawlResult(entries, page);
	}
}