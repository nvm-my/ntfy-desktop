using System.IO;
using Microsoft.Data.Sqlite;
using NtfyDesktop.Domain;

namespace NtfyDesktop.Features.History;

public class HistoryRepository
{
    private readonly string _dbPath;

    public event EventHandler<HistoryMessage>? MessageInserted;

    public HistoryRepository()
    {
        Directory.CreateDirectory(App.DataPath);
        _dbPath = Path.Combine(App.DataPath, "history.db");
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS messages (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                message_id  TEXT    NOT NULL UNIQUE,
                topic       TEXT    NOT NULL,
                timestamp   INTEGER NOT NULL,
                priority    INTEGER NOT NULL DEFAULT 3,
                title       TEXT,
                body        TEXT,
                tags        TEXT,
                click       TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_timestamp ON messages(timestamp DESC);
            CREATE INDEX IF NOT EXISTS idx_topic     ON messages(topic);
            """;
        cmd.ExecuteNonQuery();
    }

    public void Insert(NtfyMessage message)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT OR IGNORE INTO messages
                (message_id, topic, timestamp, priority, title, body, tags, click)
            VALUES
                (@mid, @topic, @ts, @priority, @title, @body, @tags, @click)
            """;
        cmd.Parameters.AddWithValue("@mid", message.Id);
        cmd.Parameters.AddWithValue("@topic", message.Topic);
        cmd.Parameters.AddWithValue("@ts", message.Time);
        cmd.Parameters.AddWithValue("@priority", (int)message.Priority);
        cmd.Parameters.AddWithValue("@title", (object?)message.Title ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@body", (object?)message.Message ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tags",
            (object?)(message.Tags?.Count > 0 ? string.Join(",", message.Tags) : null) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@click", (object?)message.Click ?? DBNull.Value);
        cmd.ExecuteNonQuery();

        var histMsg = ToHistoryMessage(message);
        MessageInserted?.Invoke(this, histMsg);

        // Retention sweeps run on a timer in HistoryRetentionService, not per-Insert.
    }

    public List<HistoryMessage> Query(
        string? topic = null,
        Priority? minPriority = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        int limit = 500)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();

        var conditions = new List<string>();
        if (topic != null) { conditions.Add("topic = @topic"); cmd.Parameters.AddWithValue("@topic", topic); }
        if (minPriority != null) { conditions.Add("priority >= @minP"); cmd.Parameters.AddWithValue("@minP", (int)minPriority.Value); }
        if (from != null) { conditions.Add("timestamp >= @from"); cmd.Parameters.AddWithValue("@from", from.Value.ToUnixTimeSeconds()); }
        if (to != null) { conditions.Add("timestamp <= @to"); cmd.Parameters.AddWithValue("@to", to.Value.ToUnixTimeSeconds()); }

        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
        cmd.CommandText = $"SELECT * FROM messages {where} ORDER BY timestamp DESC LIMIT {limit}";

        var results = new List<HistoryMessage>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add(ReadRow(reader));

        return results;
    }

    public void DeleteAll()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM messages";
        cmd.ExecuteNonQuery();
    }

    public void DeleteByTopic(string topic)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM messages WHERE topic = @topic";
        cmd.Parameters.AddWithValue("@topic", topic);
        cmd.ExecuteNonQuery();
    }

    public void DeleteByRowId(long rowId)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM messages WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", rowId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteOlderThan(int days)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days).ToUnixTimeSeconds();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM messages WHERE timestamp < @cutoff";
        cmd.Parameters.AddWithValue("@cutoff", cutoff);
        cmd.ExecuteNonQuery();
    }

    public List<string> GetDistinctTopics()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT topic FROM messages ORDER BY topic";
        var topics = new List<string>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            topics.Add(reader.GetString(0));
        return topics;
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        return conn;
    }

    private static HistoryMessage ReadRow(SqliteDataReader r)
    {
        int Col(string name) => r.GetOrdinal(name);
        string? NullStr(string name) => r.IsDBNull(Col(name)) ? null : r.GetString(Col(name));

        return new HistoryMessage
        {
            RowId = r.GetInt64(Col("id")),
            MessageId = r.GetString(Col("message_id")),
            Topic = r.GetString(Col("topic")),
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(r.GetInt64(Col("timestamp"))),
            Priority = (Priority)r.GetInt32(Col("priority")),
            Title = NullStr("title"),
            Body = NullStr("body"),
            Tags = NullStr("tags"),
            Click = NullStr("click"),
        };
    }

    private static HistoryMessage ToHistoryMessage(NtfyMessage m) => new()
    {
        MessageId = m.Id,
        Topic = m.Topic,
        Timestamp = m.Timestamp,
        Priority = m.Priority,
        Title = m.Title,
        Body = m.Message,
        Tags = m.Tags?.Count > 0 ? string.Join(",", m.Tags) : null,
        Click = m.Click,
    };
}
