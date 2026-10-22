using scraper_app.Models;
using scraper_app.TextServices;

namespace scraper_app.Filters;

public class LongTitleFilter(IWordCounter wordCounter) : IEntryFilter
{
	public string Name => "long-titles";
	public string Description => "More than five words in the title, ordered by comments (desc)";
	public IEnumerable<Entry> Apply(IEnumerable<Entry> entries) => entries.Where(x => wordCounter.Count(x.Title) > 5).OrderByDescending(e => e.Comments);
}

public class ShortTitleFilter(IWordCounter wordCounter) : IEntryFilter
{
	public string Name => "short-titles";
	public string Description => "Five or fewer words in the title, ordered by points (desc)";
	public IEnumerable<Entry> Apply(IEnumerable<Entry> entries) => entries.Where(x => wordCounter.Count(x.Title) <= 5).OrderByDescending(e => e.Points);
}