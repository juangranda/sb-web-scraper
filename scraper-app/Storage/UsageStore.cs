using System.Globalization;
using Microsoft.Data.Sqlite;
using scraper_app.Models;

namespace scraper_app.Storage;

public interface IUsageStore
{
	void Save(ScrapeRecord record);
	void PrintRecent(int limit);
}

public class UsageStore : IUsageStore
{
	readonly string _connectionString;

	public UsageStore(string path)
	{
		var directory = Path.GetDirectoryName(Path.GetFullPath(path));
		if (directory != null)
		{
			Directory.CreateDirectory(directory);
		}
		_connectionString = new SqliteConnectionStringBuilder { DataSource = path }.ToString();

		ensureSchema();
	}

	public void Save(ScrapeRecord r)
	{
		using var conn = Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = """
			INSERT INTO usage (timestamp_utc, filter, success, http_status, entries_scraped,
								entries_returned, response_bytes, fetch_ms, duration_ms, error)
			VALUES ($ts, $filter, $success, $status, $scraped, $returned, $bytes, $fetch, $duration, $error)
			""";
		cmd.Parameters.AddWithValue("$ts", r.TimestampUtc.ToString("O", CultureInfo.InvariantCulture));
		cmd.Parameters.AddWithValue("$filter", r.Filter);
		cmd.Parameters.AddWithValue("$success", r.Success);
		cmd.Parameters.AddWithValue("$status", (object?)r.HttpStatus ?? DBNull.Value);
		cmd.Parameters.AddWithValue("$scraped", r.EntriesScraped);
		cmd.Parameters.AddWithValue("$returned", r.EntriesReturned);
		cmd.Parameters.AddWithValue("$bytes", r.ResponseBytes);
		cmd.Parameters.AddWithValue("$fetch", r.FetchMs);
		cmd.Parameters.AddWithValue("$duration", r.DurationMs);
		cmd.Parameters.AddWithValue("$error", (object?)r.Error ?? DBNull.Value);
		cmd.ExecuteNonQuery();
	}

	public void PrintRecent(int limit)
	{
		using var conn = Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = """
			SELECT timestamp_utc, filter, success, http_status, entries_scraped,
					entries_returned, fetch_ms, duration_ms, error
			FROM usage ORDER BY id DESC LIMIT $limit
			""";
		cmd.Parameters.AddWithValue("$limit", limit);

		Console.WriteLine($"{"Timestamp (UTC)",-28}  {"Filter",-12}  {"OK",-3}  {"HTTP",4}  {"Scraped",7}  {"Returned",8}  {"Fetch ms",8}  {"Total ms",8}  Error");
		Console.WriteLine(new string('-', 110));
		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			var status = reader.IsDBNull(3) ? "-" : reader.GetInt32(3).ToString();
			var error = reader.IsDBNull(8) ? "" : reader.GetString(8);
			Console.WriteLine($"{reader.GetString(0),-28}  {reader.GetString(1),-12}  {(reader.GetBoolean(2) ? "yes" : "no"),-3}  {status,4}  {reader.GetInt32(4),7}  {reader.GetInt32(5),8}  {reader.GetInt64(6),8}  {reader.GetInt64(7),8}  {error}");
		}
	}

	private void ensureSchema()
	{
		using var conn = Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = """
			CREATE TABLE IF NOT EXISTS usage (
				id					INTEGER PRIMARY KEY AUTOINCREMENT,
				timestamp_utc		TEXT	NOT NULL,
				filter				TEXT	NOT NULL,
				success				INTEGER NOT NULL,
				http_status			INTEGER,
				entries_scraped		INTEGER NOT NULL,
				entries_returned	INTEGER NOT NULL,
				response_bytes		INTEGER NOT NULL,
				fetch_ms			INTEGER NOT NULL,
				duration_ms			INTEGER NOT NULL,
				error				TEXT
			)
			""";
		cmd.ExecuteNonQuery();
	}

	private SqliteConnection Open()
	{
		var conn = new SqliteConnection(_connectionString);
		conn.Open();
		return conn;
	}
}