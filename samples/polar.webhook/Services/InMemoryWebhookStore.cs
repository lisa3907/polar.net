using System.Collections.Concurrent;
using System.Text.Json;

namespace Polar.Services;

public record WebhookEventRecord(
    string EventId,
    string Type,
    DateTime CreatedAt,
    JsonElement Data
);

public interface IWebhookEventStore
{
    void Add(WebhookEventRecord record);
    IReadOnlyList<WebhookEventRecord> List(DateTime sinceUtc, string? type = null, int max = 200);
}

public sealed class InMemoryWebhookStore : IWebhookEventStore
{
    private readonly ConcurrentQueue<WebhookEventRecord> _queue = new();
    private const int MaxItems = 500;

    public void Add(WebhookEventRecord record)
    {
        _queue.Enqueue(record);
        // Trim
        while (_queue.Count > MaxItems && _queue.TryDequeue(out _)) { }
    }

    public IReadOnlyList<WebhookEventRecord> List(DateTime sinceUtc, string? type = null, int max = 200)
    {
        var items = _queue
            .Where(e => e.CreatedAt >= sinceUtc && (type == null || string.Equals(e.Type, type, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(e => e.CreatedAt)
            .Take(Math.Clamp(max, 1, 500))
            .ToList();
        return items;
    }
}
