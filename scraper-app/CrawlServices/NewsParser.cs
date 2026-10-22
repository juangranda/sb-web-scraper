using System.Text.RegularExpressions;
using HtmlAgilityPack;
using scraper_app.Models;

namespace scraper_app.CrawlServices;

public interface IEntryParser
{
	IReadOnlyList<Entry> Parse(string html, int maxEntries);
}

public class NewsParser : IEntryParser
{
	public IReadOnlyList<Entry> Parse(string html, int maxEntries)
	{
		var doc = new HtmlDocument();
		doc.LoadHtml(html);

		var entries = new List<Entry>();
		var rows = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'athing')]") ?? Enumerable.Empty<HtmlNode>();

		foreach (var row in rows.Take(maxEntries))
		{
			var number = ParseInt(row.SelectSingleNode(".//span[@class='rank']")?.InnerText);
			var title = HtmlEntity.DeEntitize(
				row.SelectSingleNode(".//span[@class='titleline']/a")?.InnerText ?? "").Trim();

			// Points and comments live in the following <tr> (td.subtext).
			var subtext = row.SelectSingleNode("following-sibling::tr[1]//td[@class='subtext']");
			var points = ParseInt(subtext?.SelectSingleNode(".//span[@class='score']")?.InnerText);
			var commentsLink = subtext?.SelectNodes(".//a")
				?.LastOrDefault(a => HtmlEntity.DeEntitize(a.InnerText).Contains("comment"));
			var comments = ParseInt(commentsLink?.InnerText);

			entries.Add(new Entry(number, title, points, comments));
		}
		return entries;
	}

// Extracts the first integer in the text ("123 points", "45&nbsp;comments", "7."); 0 if none.
	private static int ParseInt(string? text)
	{
		if (string.IsNullOrEmpty(text)) return 0;
		var match = Regex.Match(text, @"\d+");
		return match.Success ? int.Parse(match.Value) : 0;
	}
}