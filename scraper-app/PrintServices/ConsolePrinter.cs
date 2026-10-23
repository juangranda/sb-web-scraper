using scraper_app.Models;
using scraper_app.TextServices;

namespace scraper_app.PrintServices;

public interface IConsolePrinter
{
	void PrintEntries(string heading, IEnumerable<Entry> entries);
}

public class ConsolePrinter(IWordCounter wordCounter) : IConsolePrinter
{
	public void PrintEntries(string heading, IEnumerable<Entry> entries)
	{
		Console.WriteLine();
		Console.WriteLine(heading);
		Console.WriteLine($"{"#",3}  {"Points",6}  {"Comments",8}  {"Words",5}  Title");
		Console.WriteLine(new string('-', 80));
		foreach (var e in entries)
		{
			Console.WriteLine($"{e.Number,3}  {e.Points,6}  {e.Comments,8}  {wordCounter.Count(e.Title),5}  {e.Title}");
		}
	}
}