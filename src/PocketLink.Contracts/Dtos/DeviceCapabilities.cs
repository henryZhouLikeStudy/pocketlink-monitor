namespace PocketLink.Contracts.Dtos;

/// <summary>
/// 设备已发现能力矩阵，对应 PRD API-Ex 证据表。每项能力标注等级和验证状态，
/// 不代表实现方保证该能力永远可用。
/// </summary>
public sealed record DeviceCapabilities(
    string DeviceId,
    bool SupportsLogin,
    bool RequiresLoginForCoreRead,
    IReadOnlyDictionary<string, CapabilityStatus> Fields);

public enum CapabilityStatus
{
    Unknown,
    Available,
    Empty,
    Unsupported,
    LoginRequired
}
