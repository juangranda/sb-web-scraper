using Microsoft.Extensions.DependencyInjection;
using scraper_app.Application;
using scraper_app.CrawlServices;
using scraper_app.Filters;
using scraper_app.Storage;
using scraper_app.TextServices;

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