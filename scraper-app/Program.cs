using HtmlAgilityPack;
using System.Text.RegularExpressions;

const string Url = "https://news.ycombinator.com/";
const int MaxEntries = 30;

using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("HackerNewsScraper/1.0");

var html = await http.GetStringAsync(Url);
var doc = new HtmlDocument();
doc.LoadHtml(html);

var entries = new List<Entry>();
var rows = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'athing')]") ?? Enumerable.Empty<HtmlNode>();

foreach (var row in rows.Take(MaxEntries))
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

PrintTable("All entries", entries);

static void PrintTable(string heading, IEnumerable<Entry> items)
{
	Console.WriteLine();
	Console.WriteLine(heading);
	Console.WriteLine($"{"#",3}  {"Points",6}  {"Comments",8}  {"Words",5}  Title");
	Console.WriteLine(new string('-', 80));
	foreach (var e in items)
	{
		Console.WriteLine($"{e.Number,3}  {e.Points,6}  {e.Comments,8}  {CountWords(e.Title),5}  {e.Title}");
	}
}

static int CountWords(string text)
{
	var textSplit = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
	var count =	textSplit.Count(token => token.Any(char.IsLetterOrDigit));
	return count;
}

// Extracts the first integer in the text ("123 points", "45&nbsp;comments", "7."); 0 if none.
static int ParseInt(string? text)
{
	if (string.IsNullOrEmpty(text)) return 0;
	var match = Regex.Match(text, @"\d+");
	return match.Success ? int.Parse(match.Value) : 0;
}

record Entry(int Number, string Title, int Points, int Comments);