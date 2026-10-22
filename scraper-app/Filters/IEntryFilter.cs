using scraper_app.Models;

namespace scraper_app.Filters;

public interface IEntryFilter
{
	string Name { get; }
	string Description { get; }
	IEnumerable<Entry> Apply(IEnumerable<Entry> entries);
}