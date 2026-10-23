using scraper_app.TextServices;

namespace scraper_app.Tests.TextServices;

public class WordCounterTests
{
	private readonly WordCounter _counter = new();

	[Theory]
	[InlineData("This is - a self-explained example", 5)]
	[InlineData("Hello", 1)]
	[InlineData("randomSoftware 8.2.35", 2)]
	[InlineData("Show HN: Foo (YC S25)", 5)]
	[InlineData("Launch HN: Alter - Access control", 5)]
	[InlineData("a  b\tc\nd", 4)]
	[InlineData("", 0)]
	[InlineData("   ", 0)]
	[InlineData("- + — | &", 0)]
	public void Count_counts_spaced_words_and_ignores_symbol_only_tokens(string text, int expected)
	{
		Assert.Equal(expected, _counter.Count(text));
	}
}