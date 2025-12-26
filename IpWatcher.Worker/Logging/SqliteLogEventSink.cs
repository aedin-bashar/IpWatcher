using System.Text.Json;
using Microsoft.Data.Sqlite;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace IpWatcher.Worker.Logging;

public sealed class SqliteLogEventSink : ILogEventSink
{
    private readonly string _connectionString;
    private readonly JsonFormatter _jsonFormatter = new(renderMessage: true);

    public SqliteLogEventSink(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("IpWatcher")
            ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:IpWatcher'.");
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                INSERT INTO LogEvents
                (TimestampUtc, Level, MessageTemplate, RenderedMessage, Exception, PropertiesJson, LogEventJson)
                VALUES
                ($ts, $lvl, $tpl, $msg, $ex, $props, $json);
                """;

            var ts = logEvent.Timestamp.ToUniversalTime().ToString("O");
            var level = logEvent.Level.ToString();
            var template = logEvent.MessageTemplate.Text;
            var rendered = logEvent.RenderMessage();
            var ex = logEvent.Exception?.ToString();

            var props = logEvent.Properties.Count == 0
                ? null
                : JsonSerializer.Serialize(
                    logEvent.Properties.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToString()));

            string json;
            using (var sw = new StringWriter())
            {
                _jsonFormatter.Format(logEvent, sw);
                json = sw.ToString();
            }

            cmd.Parameters.AddWithValue("$ts", ts);
            cmd.Parameters.AddWithValue("$lvl", level);
            cmd.Parameters.AddWithValue("$tpl", (object?)template ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$msg", rendered);
            cmd.Parameters.AddWithValue("$ex", (object?)ex ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$props", (object?)props ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$json", json);

            cmd.ExecuteNonQuery();
        }
        catch
        {
            // DB-only logging must never crash the service.
            // If the DB is locked/unavailable/migrations not applied yet, drop the log event.
        }
    }
}