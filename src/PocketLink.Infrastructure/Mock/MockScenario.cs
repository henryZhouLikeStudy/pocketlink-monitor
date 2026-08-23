using PocketLink.Contracts.Dtos;
using PocketLink.Contracts.Interfaces;

namespace PocketLink.Infrastructure.Mock;

/// <summary>
/// Mock 场景选择，覆盖 PRD 6.4/7 中要求验证的关键分支：
/// 正常取值、5G SA 回退 LTE RSRP、字段为空、字段设备不支持。
/// </summary>
public enum MockScenario
{
    Normal,
    SaFallbackToLte,
    EmptyFields,
    Unsupported
}

public sealed class MockDeviceAdapterOptions
{
    public string DeviceId { get; init; } = "mock-mu5353-001";

    public MockScenario Scenario { get; init; } = MockScenario.Normal;

    /// <summary>
    /// 固定种子，保证同一场景下多次运行产生完全一致的数值序列，便于测试断言。
    /// </summary>
    public int Seed { get; init; } = 20260823;

    /// <summary>
    /// 注入的时钟起点；未提供时使用 DateTimeOffset.UnixEpoch 加上调用计数偏移，避免测试依赖真实系统时间。
    /// </summary>
    public DateTimeOffset BaseTimeUtc { get; init; } = new DateTimeOffset(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);
}
