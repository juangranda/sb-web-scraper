using scraper_app.CrawlServices;
using scraper_app.Models;
using scraper_app.Tests.Fakes;

namespace scraper_app.Tests.CrawlingServices;

public class NewsCrawlerTests
{
	[Fact]
	public async Task CrawlAsync_fetches_configured_url_and_parses_up_to_max_entries()
	{
		var page = new FetchedPage("<html/>", 200, 7, TimeSpan.FromMilliseconds(100));
		var entries = new List<Entry> { new(1, "Title", 10, 2) };
		var fetcher = new FakePageFetcher(page);
		var parser = new FakeNewsParser(entries);

		var result = await new NewsCrawler(fetcher, parser).CrawlAsync("http://example.com", 5);

		Assert.Same(page, result.Page);
		Assert.Same(entries, result.Entries);
	}
}