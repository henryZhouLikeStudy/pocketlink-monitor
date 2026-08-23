namespace PocketLink.Contracts.Dtos;

/// <summary>
/// 实时吞吐样本。设备原始字段单位未确认前必须携带 UnitVerified=false，UI 显示“单位待验证”。
/// </summary>
public sealed record ThroughputSample(
    MetricSample Download,
    MetricSample Upload,
    bool UnitVerified);
