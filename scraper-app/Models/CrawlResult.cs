namespace scraper_app.Models;

public record FetchedPage(string Html, int StatusCode, long ResponseBytes, TimeSpan Elapsed);

public record CrawlResult(IReadOnlyList<Entry> Entries, FetchedPage Page);
