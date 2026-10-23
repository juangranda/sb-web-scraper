using scraper_app.Filters;
using scraper_app.Models;
using scraper_app.TextServices;

namespace scraper_app.Tests.Filters;

public class FilterTests
{
	private static readonly WordCounter Counter = new();

	private static readonly Entry FiveWords = new(1, "one two three four five", Points: 50, Comments: 5);
	private static readonly Entry SixWords = new(2, "one two three four five six", Points: 10, Comments: 90);
	private static readonly Entry TwoWords = new(3, "autopolitan outlines", Points: 300, Comments: 1);
	private static readonly Entry LongWithSymbols = new(4, "Show HN: a - b - c - d", Points: 1, Comments: 40); // 6 words
	private static readonly Entry FourWithDash = new(5, "a – b – c – d", Points: 70, Comments: 0);         // 4 words

	private static readonly Entry[] All = [FiveWords, SixWords, TwoWords, LongWithSymbols, FourWithDash];

	[Fact]
	public void LongTitleFilter_keeps_titles_over_five_words_sorted_by_comments_desc()
	{
		var result = new LongTitleFilter(Counter).Apply(All);

		Assert.Equal([SixWords, LongWithSymbols], result);
	}
}