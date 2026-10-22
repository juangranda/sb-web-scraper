namespace scraper_app.TextServices;

public interface IWordCounter
{
	int Count(string text);
}

public class WordCounter : IWordCounter
{
	public int Count(string text)
	{
		var textSplit = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
		var count =	textSplit.Count(token => token.Any(char.IsLetterOrDigit));
		return count;
	}
}