using System.Diagnostics;
using System.Text;
using scraper_app.Models;

namespace scraper_app.CrawlServices;

public interface IPageFetcher
{
	Task<FetchedPage> FetchAsync(string url, CancellationToken cancellationToken = default);
}

public class HttpPageFetcher : IPageFetcher
{
	private readonly HttpClient _httpClient;

	public HttpPageFetcher(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<FetchedPage> FetchAsync(string url, CancellationToken cancellationToken = default)
	{
		var stopwatch = Stopwatch.StartNew();
		var response = await _httpClient.GetAsync(url, cancellationToken);
		response.EnsureSuccessStatusCode();

		var html = await response.Content.ReadAsStringAsync(cancellationToken);
		var statusCode = (int)response.StatusCode;
		var responseBytes = Encoding.UTF8.GetByteCount(html);
		var elapsed = stopwatch.Elapsed;

		return new FetchedPage(html, statusCode, responseBytes, elapsed);
	}
}