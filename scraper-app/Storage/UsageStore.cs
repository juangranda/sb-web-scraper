using System.Globalization;
using Microsoft.Data.Sqlite;
using scraper_app.Models;

namespace scraper_app.Storage;

public interface IUsageStore
{
	Task SaveAsync(ScrapeRecord record, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<ScrapeRecord>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
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

		EnsureSchema();
	}

	public async Task SaveAsync(ScrapeRecord r, CancellationToken cancellationToken = default)
	{
		await using var conn = await OpenAsync(cancellationToken);
		await using var cmd = conn.CreateCommand();
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
		await cmd.ExecuteNonQueryAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<ScrapeRecord>> GetRecentAsync(int limit, CancellationToken cancellationToken = default)
	{
		await using var conn = await OpenAsync(cancellationToken);
		await using var cmd = conn.CreateCommand();
		cmd.CommandText = """
			SELECT timestamp_utc, filter, success, http_status, entries_scraped,
					entries_returned, response_bytes, fetch_ms, duration_ms, error
			FROM usage ORDER BY id DESC LIMIT $limit
			""";
		cmd.Parameters.AddWithValue("$limit", limit);

		var records = new List<ScrapeRecord>();

		await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			var record = new ScrapeRecord
			{
				TimestampUtc = DateTime.Parse(reader.GetString(0), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
				Filter = reader.GetString(1),
				Success = reader.GetBoolean(2),
				HttpStatus = reader.IsDBNull(3) ? null : reader.GetInt32(3),
				EntriesScraped = reader.GetInt32(4),
				EntriesReturned = reader.GetInt32(5),
				ResponseBytes = reader.GetInt64(6),
				FetchMs = reader.GetInt64(7),
				DurationMs = reader.GetInt64(8),
				Error = reader.IsDBNull(9) ? null : reader.GetString(9)
			};
			records.Add(record);
		}
		return records;
	}

	private void EnsureSchema()
	{
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

	private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken = default)
	{
		var conn = new SqliteConnection(_connectionString);
		await conn.OpenAsync(cancellationToken);
		return conn;
	}
}