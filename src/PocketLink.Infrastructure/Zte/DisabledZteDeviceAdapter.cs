using PocketLink.Contracts.Dtos;
using PocketLink.Contracts.Interfaces;

namespace PocketLink.Infrastructure.Zte;

/// <summary>
/// 占位 ZTE 适配器：显式禁用实时设备传输。
/// 当前 MVP 不访问 192.168.* 地址，不读取凭据，不调用设备写接口，不执行刷机、配置写入或重启。
/// 本类仅用于测试验证只读契约边界；真实 HTTP 传输未实现。
/// </summary>
public sealed class DisabledZteDeviceAdapter : IZteDeviceAdapter
{
    private readonly string _baseUrl;

    public DisabledZteDeviceAdapter(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public string DeviceId => $"zte-disabled-{_baseUrl.GetHashCode():X8}";

    public Task<bool> ProbeAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "DisabledZteDeviceAdapter: 实时设备传输已禁用。此 MVP 版本不访问真实设备，不读取凭据，不执行网络请求。");
    }

    public Task<DeviceCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "DisabledZteDeviceAdapter: 实时设备传输已禁用。此 MVP 版本不访问真实设备，不读取凭据，不执行网络请求。");
    }

    public Task<DeviceSnapshot> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "DisabledZteDeviceAdapter: 实时设备传输已禁用。此 MVP 版本不访问真实设备，不读取凭据，不执行网络请求。");
    }

    public Task<MetricSample> ReadRawFieldAsync(string fieldName, CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "DisabledZteDeviceAdapter: 实时设备传输已禁用。此 MVP 版本不访问真实设备，不读取凭据，不执行网络请求。");
    }

    public Task<IReadOnlyDictionary<string, MetricSample>> ReadRawFieldsAsync(
        IReadOnlyCollection<string> fieldNames,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "DisabledZteDeviceAdapter: 实时设备传输已禁用。此 MVP 版本不访问真实设备，不读取凭据，不执行网络请求。");
    }
}
