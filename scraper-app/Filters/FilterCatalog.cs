using System.Diagnostics.CodeAnalysis;

namespace scraper_app.Filters;

public interface IFilterCatalog
{
	IEnumerable<string> Names { get; }
	bool TryGetFilter(string name, [NotNullWhen(true)] out IEntryFilter? filter);
}

public class FilterCatalog(IEnumerable<IEntryFilter> filters) : IFilterCatalog
{
	private readonly Dictionary<string, IEntryFilter> _filters = filters.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

	public IEnumerable<string> Names => _filters.Keys;

	public bool TryGetFilter(string name, [NotNullWhen(true)] out IEntryFilter? filter) => _filters.TryGetValue(name, out filter);
}