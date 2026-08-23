using PocketLink.Contracts.Dtos;
using PocketLink.Contracts.Interfaces;

namespace PocketLink.Infrastructure.Mock;

/// <summary>
/// 确定性模拟适配器：给定相同 Seed + Scenario + 调用序号，产出完全一致的样本序列。
/// 不访问任何网络/设备，仅用于开发和测试。真实 SDK 适配器应实现同一 IDeviceAdapter/IZteDeviceAdapter
/// 契约并替换本类，无需改动上层消费代码。
/// </summary>
public sealed class MockDeviceAdapter : IZteDeviceAdapter
{
    private readonly MockDeviceAdapterOptions _options;
    private int _callIndex;

    public MockDeviceAdapter(MockDeviceAdapterOptions? options = null)
    {
        _options = options ?? new MockDeviceAdapterOptions();
    }

    public string DeviceId => _options.DeviceId;

    public Task<bool> ProbeAsync(CancellationToken cancellationToken) => Task.FromResult(true);

    public Task<DeviceCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, CapabilityStatus>
        {
            ["network_lte_rsrp"] = CapabilityStatus.Available,
            ["Z5g_rsrp"] = _options.Scenario == MockScenario.Unsupported
                ? CapabilityStatus.Unsupported
                : CapabilityStatus.Available,
            ["Z5g_SINR"] = _options.Scenario == MockScenario.EmptyFields
                ? CapabilityStatus.Empty
                : CapabilityStatus.Available,
            ["flux_realtime_tx_thrpt"] = CapabilityStatus.Available,
            ["flux_realtime_rx_thrpt"] = CapabilityStatus.Available,
            ["wifi_chip_temp"] = CapabilityStatus.Unsupported,
            ["battery_vol_percent"] = CapabilityStatus.Available,
        };

        return Task.FromResult(new DeviceCapabilities(
            DeviceId,
            SupportsLogin: false,
            RequiresLoginForCoreRead: false,
            fields));
    }

    public Task<DeviceSnapshot> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        var index = Interlocked.Increment(ref _callIndex);
        var now = _options.BaseTimeUtc.AddSeconds(index);
        var rng = CreateDeterministicRandom(index);

        var connection = BuildConnectionSummary(now);
        var signal = BuildSignalSample(now, rng);
        var throughput = BuildThroughputSample(now, rng);
        var events = BuildEvents(now, signal);

        return Task.FromResult(new DeviceSnapshot(DeviceId, now, connection, signal, throughput, events));
    }

    public Task<MetricSample> ReadRawFieldAsync(string fieldName, CancellationToken cancellationToken)
    {
        var index = Interlocked.Increment(ref _callIndex);
        var now = _options.BaseTimeUtc.AddSeconds(index);
        var rng = CreateDeterministicRandom(index);

        return Task.FromResult(BuildRawFieldSample(fieldName, now, rng));
    }

    public async Task<IReadOnlyDictionary<string, MetricSample>> ReadRawFieldsAsync(
        IReadOnlyCollection<string> fieldNames,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, MetricSample>();
        foreach (var name in fieldNames)
        {
            result[name] = await ReadRawFieldAsync(name, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// 种子 = 用户配置 Seed 与调用序号组合，保证同一场景重复运行产出一致数列，
    /// 且不同调用序号之间数值有变化（用于吞吐历史图）。
    /// </summary>
    private Random CreateDeterministicRandom(int callIndex) => new(HashCode.Combine(_options.Seed, callIndex));

    private ConnectionSummary BuildConnectionSummary(DateTimeOffset now)
    {
        if (_options.Scenario == MockScenario.Unsupported)
        {
            return new ConnectionSummary(
                DeviceId,
                ModelName: "MU5353",
                FirmwareVersion: "BD_CNMU5353V1.0.0B07",
                NetworkType: NetworkType.Sa,
                PppStatus: "ipv4_ipv6_connected",
                Wifi24Enabled: null,
                Wifi5Enabled: null,
                BatteryPercent: MetricSample.Unsupported(DeviceId, now, SampleSource.DeviceApi, "%"));
        }

        var networkType = _options.Scenario == MockScenario.SaFallbackToLte ? NetworkType.Sa : NetworkType.Sa;

        return new ConnectionSummary(
            DeviceId,
            ModelName: "MU5353",
            FirmwareVersion: "BD_CNMU5353V1.0.0B07",
            NetworkType: networkType,
            PppStatus: "ipv4_ipv6_connected",
            Wifi24Enabled: false,
            Wifi5Enabled: true,
            BatteryPercent: new MetricSample(
                DeviceId, now, now, SampleSource.DeviceApi, SampleStatus.Valid,
                RawValue: "49", NormalizedValue: 49, Unit: "%"));
    }

    private SignalSample BuildSignalSample(DateTimeOffset now, Random rng)
    {
        var lteRsrpValue = -92 + rng.Next(-3, 4);
        var lteRsrp = new MetricSample(
            DeviceId, now, now, SampleSource.DeviceApi, SampleStatus.Valid,
            RawValue: lteRsrpValue.ToString(), NormalizedValue: lteRsrpValue, Unit: "dBm");

        MetricSample? z5GRsrp = _options.Scenario switch
        {
            MockScenario.SaFallbackToLte => MetricSample.Empty(DeviceId, now, SampleSource.DeviceApi, "dBm"),
            MockScenario.Unsupported => MetricSample.Unsupported(DeviceId, now, SampleSource.DeviceApi, "dBm"),
            _ => new MetricSample(
                DeviceId, now, now, SampleSource.DeviceApi, SampleStatus.Valid,
                RawValue: (-99 + rng.Next(-2, 3)).ToString(),
                NormalizedValue: -99 + rng.Next(-2, 3),
                Unit: "dBm"),
        };

        MetricSample? sinr = _options.Scenario == MockScenario.EmptyFields
            ? MetricSample.Empty(DeviceId, now, SampleSource.DeviceApi, "dB")
            : new MetricSample(
                DeviceId, now, now, SampleSource.DeviceApi, SampleStatus.Valid,
                RawValue: "18", NormalizedValue: 18, Unit: "dB");

        return SignalSample.Select(DeviceId, now, NetworkType.Sa, z5GRsrp, lteRsrp, sinr);
    }

    private ThroughputSample BuildThroughputSample(DateTimeOffset now, Random rng)
    {
        if (_options.Scenario == MockScenario.EmptyFields)
        {
            return new ThroughputSample(
                MetricSample.Empty(DeviceId, now, SampleSource.DeviceApi, "unit_unverified"),
                MetricSample.Empty(DeviceId, now, SampleSource.DeviceApi, "unit_unverified"),
                UnitVerified: false);
        }

        var download = new MetricSample(
            DeviceId, now, now, SampleSource.DeviceApi, SampleStatus.Valid,
            RawValue: rng.Next(30000, 90000).ToString(),
            NormalizedValue: rng.Next(30000, 90000),
            Unit: "unit_unverified");

        var upload = new MetricSample(
            DeviceId, now, now, SampleSource.DeviceApi, SampleStatus.Valid,
            RawValue: rng.Next(400000, 1300000).ToString(),
            NormalizedValue: rng.Next(400000, 1300000),
            Unit: "unit_unverified");

        return new ThroughputSample(download, upload, UnitVerified: false);
    }

    private IReadOnlyList<EventEntry> BuildEvents(DateTimeOffset now, SignalSample signal)
    {
        var events = new List<EventEntry>
        {
            new(now, EventSeverity.Info, "模拟快照已刷新", "sampling"),
        };

        if (signal.SourceFallback)
        {
            events.Add(new EventEntry(now, EventSeverity.Warning, "Z5g_rsrp 为空，已回退至 network_lte_rsrp", "signal_source"));
        }

        if (_options.Scenario == MockScenario.Unsupported)
        {
            events.Add(new EventEntry(now, EventSeverity.Warning, "wifi_chip_temp 字段设备不支持", "capability"));
        }

        return events;
    }

    private MetricSample BuildRawFieldSample(string fieldName, DateTimeOffset now, Random rng)
    {
        if (_options.Scenario == MockScenario.Unsupported && fieldName == "wifi_chip_temp")
        {
            return MetricSample.Unsupported(DeviceId, now, SampleSource.DeviceApi);
        }

        if (_options.Scenario == MockScenario.EmptyFields)
        {
            return MetricSample.Empty(DeviceId, now, SampleSource.DeviceApi);
        }

        var value = rng.Next(0, 100);
        return new MetricSample(
            DeviceId, now, now, SampleSource.DeviceApi, SampleStatus.Valid,
            RawValue: value.ToString(), NormalizedValue: value, Unit: null);
    }
}
