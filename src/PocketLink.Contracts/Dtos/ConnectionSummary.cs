namespace PocketLink.Contracts.Dtos;

/// <summary>
/// 连接摘要卡片数据：设备型号/固件、网络类型、PPP 状态、Wi-Fi 开关摘要。
/// 任意字段为 null 表示当前读取为空，不得替换为占位假值。
/// </summary>
public sealed record ConnectionSummary(
    string DeviceId,
    string? ModelName,
    string? FirmwareVersion,
    NetworkType NetworkType,
    string? PppStatus,
    bool? Wifi24Enabled,
    bool? Wifi5Enabled,
    MetricSample? BatteryPercent);
