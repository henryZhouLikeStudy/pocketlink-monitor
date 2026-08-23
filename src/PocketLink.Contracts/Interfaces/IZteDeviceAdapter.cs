using PocketLink.Contracts.Dtos;

namespace PocketLink.Contracts.Interfaces;

/// <summary>
/// ZTE MU5353 专用只读扩展，暴露 phase0-evidence-20260823.md 中已验证的原始字段读取，
/// 供上层在通用快照之外做设备特定诊断。不包含任何写操作。
/// </summary>
public interface IZteDeviceAdapter : IDeviceAdapter
{
    Task<MetricSample> ReadRawFieldAsync(string fieldName, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, MetricSample>> ReadRawFieldsAsync(
        IReadOnlyCollection<string> fieldNames,
        CancellationToken cancellationToken);
}
