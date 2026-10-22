public class ScrapeRecord
{
	public DateTime TimestampUtc { get; set; }
	public string Filter { get; set; } = "";
	public bool Success { get; set; }
	public int? HttpStatus { get; set; }
	public int EntriesScraped { get; set; }
	public int EntriesReturned { get; set; }
	public long ResponseBytes { get; set; }
	public long FetchMs { get; set; }
	public long DurationMs { get; set; }
	public string? Error { get; set; }
}