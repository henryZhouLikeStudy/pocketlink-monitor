namespace PocketLink.Contracts.Dtos;

/// <summary>
/// 事件条目，用于底部紧凑事件带。Severity 仅描述展示优先级，不代表设备侧告警等级。
/// </summary>
public sealed record EventEntry(
    DateTimeOffset TimestampUtc,
    EventSeverity Severity,
    string Message,
    string? Category);

public enum EventSeverity
{
    Info,
    Warning,
    Error
}
