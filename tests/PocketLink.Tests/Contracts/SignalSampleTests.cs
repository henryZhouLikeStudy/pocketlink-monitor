using PocketLink.Contracts.Dtos;
using Xunit;

namespace PocketLink.Tests.Contracts;

public sealed class SignalSampleTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);
    private const string DeviceId = "device-1";

    [Fact]
    public void Select_SaWithZ5gRsrpValid_UsesZ5gAndNoFallback()
    {
        var z5g = Valid(-99, "dBm");
        var lte = Valid(-92, "dBm");

        var result = SignalSample.Select(DeviceId, Now, NetworkType.Sa, z5g, lte, sinr: null);

        Assert.Equal(SignalFieldOrigin.Z5gRsrp, result.RsrpOrigin);
        Assert.False(result.SourceFallback);
        Assert.Equal(-99, result.Rsrp.NormalizedValue);
    }

    [Fact]
    public void Select_SaWithEmptyZ5gRsrp_FallsBackToLteAndFlagsFallback()
    {
        var z5g = MetricSample.Empty(DeviceId, Now, SampleSource.DeviceApi, "dBm");
        var lte = Valid(-92, "dBm");

        var result = SignalSample.Select(DeviceId, Now, NetworkType.Sa, z5g, lte, sinr: null);

        Assert.Equal(SignalFieldOrigin.NetworkLteRsrp, result.RsrpOrigin);
        Assert.True(result.SourceFallback);
        Assert.Equal(-92, result.Rsrp.NormalizedValue);
    }

    [Fact]
    public void Select_SaWithBothEmpty_ReturnsEmptySampleWithoutFallbackFlag()
    {
        var z5g = MetricSample.Empty(DeviceId, Now, SampleSource.DeviceApi, "dBm");
        var lte = MetricSample.Empty(DeviceId, Now, SampleSource.DeviceApi, "dBm");

        var result = SignalSample.Select(DeviceId, Now, NetworkType.Sa, z5g, lte, sinr: null);

        Assert.Equal(SampleStatus.Empty, result.Rsrp.Status);
        Assert.Null(result.Rsrp.NormalizedValue);
        Assert.False(result.SourceFallback);
        Assert.Equal(SignalFieldOrigin.None, result.RsrpOrigin);
    }

    [Fact]
    public void Select_LteNetwork_PrefersNetworkLteRsrpWithoutFallbackFlag()
    {
        var lte = Valid(-85, "dBm");

        var result = SignalSample.Select(DeviceId, Now, NetworkType.Lte, z5GRsrp: null, lte, sinr: null);

        Assert.Equal(SignalFieldOrigin.NetworkLteRsrp, result.RsrpOrigin);
        Assert.False(result.SourceFallback);
        Assert.Equal(-85, result.Rsrp.NormalizedValue);
    }

    [Fact]
    public void Select_LteNetworkWithNoRsrp_ReturnsEmpty()
    {
        var result = SignalSample.Select(DeviceId, Now, NetworkType.Lte, z5GRsrp: null, lteRsrp: null, sinr: null);

        Assert.Equal(SampleStatus.Empty, result.Rsrp.Status);
        Assert.Equal(SignalFieldOrigin.None, result.RsrpOrigin);
    }

    private static MetricSample Valid(double value, string unit) =>
        new(DeviceId, Now, Now, SampleSource.DeviceApi, SampleStatus.Valid, value.ToString(), value, unit);
}
