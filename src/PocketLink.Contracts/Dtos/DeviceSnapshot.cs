namespace PocketLink.Contracts.Dtos;

/// <summary>
/// 单次快照聚合结果，供仪表盘一次性绑定。
/// </summary>
public sealed record DeviceSnapshot(
    string DeviceId,
    DateTimeOffset ObservedAtUtc,
    ConnectionSummary Connection,
    SignalSample Signal,
    ThroughputSample Throughput,
    IReadOnlyList<EventEntry> RecentEvents);
