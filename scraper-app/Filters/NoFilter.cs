using scraper_app.Models;

namespace scraper_app.Filters;

public class NoFilter : IEntryFilter
{
	public const string FilterName = "none";
	public string Name => FilterName;
	public string Description => "All entries";
	public IEnumerable<Entry> Apply(IEnumerable<Entry> entries) => entries;
}