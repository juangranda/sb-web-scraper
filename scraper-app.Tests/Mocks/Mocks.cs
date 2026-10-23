using scraper_app.CrawlServices;
using scraper_app.Models;

namespace scraper_app.Tests.Fakes;

public class FakePageFetcher(FetchedPage page) : IPageFetcher
{
	public string? RequestedUrl { get; set; }

	public Task<FetchedPage> FetchAsync(string url, CancellationToken cancellationToken = default)
	{
		RequestedUrl = url;
		return Task.FromResult(page);
	}
}

public class FakeNewsParser(IReadOnlyList<Entry> entries) : IEntryParser
{
	public string? PrsedHtml { get; set; }
	public int RequestedMax { get; set; }

	public IReadOnlyList<Entry> Parse(string html, int maxEntries)
	{
		PrsedHtml = html;
		RequestedMax = maxEntries;
		return entries;
	}
}

public class StubHttpMessageHandler(HttpStatusCode status, string content) : HttpMessageHandler
{
	public string? Url { get; set; }

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Url = request.RequestUri?.ToString() ?? string.Empty;
		var response = new HttpResponseMessage(status)
		{
			Content = new StringContent(content)
		};
		return Task.FromResult(response);
	}
}