namespace PocketLink.Contracts.Dtos;

/// <summary>
/// 通用度量样本，字段对应 PRD 6.4：device_id、observed_at_utc、received_at_utc、source、status、raw_value、normalized_value、unit。
/// RawValue 保留设备原始字符串，避免把空值伪造成 0；NormalizedValue 仅在 Status = Valid 时应视为可信。
/// </summary>
public sealed record MetricSample(
    string DeviceId,
    DateTimeOffset ObservedAtUtc,
    DateTimeOffset ReceivedAtUtc,
    SampleSource Source,
    SampleStatus Status,
    string? RawValue,
    double? NormalizedValue,
    string? Unit)
{
    public static MetricSample Empty(string deviceId, DateTimeOffset now, SampleSource source, string? unit = null) =>
        new(deviceId, now, now, source, SampleStatus.Empty, RawValue: null, NormalizedValue: null, Unit: unit);

    public static MetricSample Unsupported(string deviceId, DateTimeOffset now, SampleSource source, string? unit = null) =>
        new(deviceId, now, now, source, SampleStatus.Unsupported, RawValue: null, NormalizedValue: null, Unit: unit);

    public static MetricSample ReadFailure(string deviceId, DateTimeOffset now, SampleSource source, string? unit = null) =>
        new(deviceId, now, now, source, SampleStatus.ReadFailed, RawValue: null, NormalizedValue: null, Unit: unit);
}
