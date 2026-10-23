using scraper_app.CrawlServices;
using scraper_app.Tests.Fakes;

namespace scraper_app.Tests.CrawlingServices;

public class HttpPageFetcherTests
{
	static readonly string Url = "https://news.example.test/";

	[Fact]
	public async Task FetchAsync_returns_html_status_and_byte_size()
	{
		var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "<p>autopolitan</p>");
		var fetcher = new HttpPageFetcher(new HttpClient(handler));

		var page = await fetcher.FetchAsync(Url);

		Assert.Equal(Url, handler.Url);
		Assert.Equal("<p>autopolitan</p>", page.Html);
		Assert.Equal(200, page.StatusCode);
		Assert.Equal(18, page.ResponseBytes);
	}
}