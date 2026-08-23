using PocketLink.Contracts.Dtos;

namespace PocketLink.Contracts.Interfaces;

/// <summary>
/// 只读设备适配器契约。首版只允许只读操作；PreviewCommand/ExecuteCommand/VerifyCommand
/// 属于写路径，在真机写接口证据到位前不得在此接口中实现或调用。
/// </summary>
public interface IDeviceAdapter
{
    string DeviceId { get; }

    Task<bool> ProbeAsync(CancellationToken cancellationToken);

    Task<DeviceCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken);

    Task<DeviceSnapshot> ReadSnapshotAsync(CancellationToken cancellationToken);
}
