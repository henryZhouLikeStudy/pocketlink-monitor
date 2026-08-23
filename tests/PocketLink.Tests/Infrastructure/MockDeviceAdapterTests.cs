using PocketLink.Contracts.Dtos;
using PocketLink.Infrastructure.Mock;
using Xunit;

namespace PocketLink.Tests.Infrastructure;

public sealed class MockDeviceAdapterTests
{
    [Fact]
    public async Task ReadSnapshotAsync_SameSeedAndScenario_ProducesDeterministicSequence()
    {
        var options = new MockDeviceAdapterOptions { Seed = 42, Scenario = MockScenario.Normal };
        var a = new MockDeviceAdapter(options);
        var b = new MockDeviceAdapter(options);

        var s1a = await a.ReadSnapshotAsync(CancellationToken.None);
        var s1b = await b.ReadSnapshotAsync(CancellationToken.None);

        Assert.Equal(s1a.Signal.Rsrp.NormalizedValue, s1b.Signal.Rsrp.NormalizedValue);
        Assert.Equal(s1a.Throughput.Download.NormalizedValue, s1b.Throughput.Download.NormalizedValue);
        Assert.Equal(s1a.ObservedAtUtc, s1b.ObservedAtUtc);
    }

    [Fact]
    public async Task ReadSnapshotAsync_SaFallbackScenario_MarksSourceFallback()
    {
        var adapter = new MockDeviceAdapter(new MockDeviceAdapterOptions { Scenario = MockScenario.SaFallbackToLte });

        var snapshot = await adapter.ReadSnapshotAsync(CancellationToken.None);

        Assert.True(snapshot.Signal.SourceFallback);
        Assert.Equal(SignalFieldOrigin.NetworkLteRsrp, snapshot.Signal.RsrpOrigin);
        Assert.Equal(SampleStatus.Valid, snapshot.Signal.Rsrp.Status);
        Assert.Contains(snapshot.RecentEvents, e => e.Category == "signal_source");
    }

    [Fact]
    public async Task ReadSnapshotAsync_EmptyFieldsScenario_ReturnsEmptyStatusNotZero()
    {
        var adapter = new MockDeviceAdapter(new MockDeviceAdapterOptions { Scenario = MockScenario.EmptyFields });

        var snapshot = await adapter.ReadSnapshotAsync(CancellationToken.None);

        Assert.Equal(SampleStatus.Empty, snapshot.Signal.Sinr!.Status);
        Assert.Null(snapshot.Signal.Sinr!.NormalizedValue);
        Assert.Equal(SampleStatus.Empty, snapshot.Throughput.Download.Status);
        Assert.False(snapshot.Throughput.UnitVerified);
    }

    [Fact]
    public async Task ReadSnapshotAsync_UnsupportedScenario_BatteryAndTempMarkedUnsupported()
    {
        var adapter = new MockDeviceAdapter(new MockDeviceAdapterOptions { Scenario = MockScenario.Unsupported });

        var snapshot = await adapter.ReadSnapshotAsync(CancellationToken.None);
        var temp = await adapter.ReadRawFieldAsync("wifi_chip_temp", CancellationToken.None);

        Assert.Equal(SampleStatus.Unsupported, snapshot.Connection.BatteryPercent!.Status);
        Assert.Null(snapshot.Connection.Wifi24Enabled);
        Assert.Equal(SampleStatus.Unsupported, temp.Status);
    }

    [Fact]
    public async Task GetCapabilitiesAsync_ReflectsScenarioFieldStatus()
    {
        var adapter = new MockDeviceAdapter(new MockDeviceAdapterOptions { Scenario = MockScenario.Unsupported });

        var caps = await adapter.GetCapabilitiesAsync(CancellationToken.None);

        Assert.Equal(CapabilityStatus.Unsupported, caps.Fields["Z5g_rsrp"]);
        Assert.False(caps.RequiresLoginForCoreRead);
    }

    [Fact]
    public async Task ProbeAsync_AlwaysSucceeds()
    {
        var adapter = new MockDeviceAdapter();
        Assert.True(await adapter.ProbeAsync(CancellationToken.None));
    }
}
