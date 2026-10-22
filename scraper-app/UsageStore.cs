using System.Globalization;
using Microsoft.Data.Sqlite;

public class UsageStore
{
	readonly string _connectionString;

	public UsageStore(string path)
	{
		_connectionString = new SqliteConnectionStringBuilder { DataSource = path }.ToString();

		using var conn = new SqliteConnection(_connectionString);
		conn.Open();
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

	public void Save(ScrapeRecord r)
	{
		using var conn = new SqliteConnection(_connectionString);
		conn.Open();
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
}