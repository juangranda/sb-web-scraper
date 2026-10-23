using scraper_app.Models;
using scraper_app.TextServices;

namespace scraper_app.PrintServices;

public interface IConsolePrinter
{
	void PrintEntries(string heading, IEnumerable<Entry> entries);
	void PrintHistory(IEnumerable<ScrapeRecord> records);
	void PrintError(string message);
}

public class ConsolePrinter(IWordCounter wordCounter) : IConsolePrinter
{
	public void PrintEntries(string heading, IEnumerable<Entry> entries)
	{
		Console.WriteLine();
		Console.WriteLine($"Filter: {heading}");
		Console.WriteLine($"{"#",3}  {"Points",6}  {"Comments",8}  {"Words",5}  Title");
		Console.WriteLine(new string('-', 80));
		foreach (var e in entries)
		{
			Console.WriteLine($"{e.Number,3}  {e.Points,6}  {e.Comments,8}  {wordCounter.Count(e.Title),5}  {e.Title}");
		}
	}

	public void PrintHistory(IEnumerable<ScrapeRecord> records)
	{
		Console.WriteLine();
		Console.WriteLine("Recent Usage:");
		Console.WriteLine($"{"Timestamp (UTC)",-28}  {"Filter",-12}  {"OK",-3}  {"HTTP",4}  {"Scraped",7}  {"Returned",8}  {"Fetch ms",8}  {"Total ms",8}  Error");
		Console.WriteLine(new string('-', 110));

		foreach (var r in records)
		{
			var status = r.HttpStatus?.ToString() ?? "-";
			var error = string.IsNullOrEmpty(r.Error) ? "-" : r.Error;
			Console.WriteLine($"{r.TimestampUtc,-28}  {r.Filter,-12}  {(r.Success ? "yes" : "no"),-3}  {status,4}  {r.EntriesScraped,7}  {r.EntriesReturned,8}  {r.FetchMs,8}  {r.DurationMs,8}  {error}");
		}
	}

	public void PrintError(string message)
	{
		Console.WriteLine();
		Console.Error.WriteLine($"Error: {message}");
	}
}