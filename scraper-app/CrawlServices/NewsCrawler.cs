using scraper_app.Models;

namespace scraper_app.CrawlServices;

public interface INewsCrawler
{
	Task<CrawlResult> CrawlAsync(string url, int maxEntries, CancellationToken cancellationToken = default);
}

public class NewsCrawler : INewsCrawler
{
	private readonly IPageFetcher _fetcher;
	private readonly IEntryParser _parser;

	public NewsCrawler(IPageFetcher fectcher, IEntryParser parser)
	{
		_fetcher = fectcher;
		_parser = parser;
	}

	public async Task<CrawlResult> CrawlAsync(string url, int maxEntries, CancellationToken cancellationToken = default)
	{
		var page = await _fetcher.FetchAsync(url, cancellationToken);
		var entries = _parser.Parse(page.Html, maxEntries);
		return new CrawlResult(entries, page);
	}
}