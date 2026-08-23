namespace PocketLink.Contracts.Dtos;

/// <summary>
/// 5G SA/LTE 信号选源结果。对应 PRD 6.4 选源规则：
/// network_type=SA 时优先 Z5g_rsrp，为空则回退 network_lte_rsrp 并置 SourceFallback=true。
/// LTE/NSA 时优先 network_lte_rsrp。两者都有值时分别保留，不静默混合。
/// </summary>
public sealed record SignalSample(
    MetricSample Rsrp,
    MetricSample? Sinr,
    NetworkType NetworkType,
    SignalFieldOrigin RsrpOrigin,
    bool SourceFallback)
{
    public static SignalSample Select(
        string deviceId,
        DateTimeOffset now,
        NetworkType networkType,
        MetricSample? z5GRsrp,
        MetricSample? lteRsrp,
        MetricSample? sinr)
    {
        if (networkType == NetworkType.Sa)
        {
            if (z5GRsrp is { Status: SampleStatus.Valid })
            {
                return new SignalSample(z5GRsrp, sinr, networkType, SignalFieldOrigin.Z5gRsrp, SourceFallback: false);
            }

            if (lteRsrp is { Status: SampleStatus.Valid })
            {
                return new SignalSample(lteRsrp, sinr, networkType, SignalFieldOrigin.NetworkLteRsrp, SourceFallback: true);
            }

            return new SignalSample(
                MetricSample.Empty(deviceId, now, SampleSource.DeviceApi, "dBm"),
                sinr,
                networkType,
                SignalFieldOrigin.None,
                SourceFallback: false);
        }

        if (lteRsrp is { Status: SampleStatus.Valid })
        {
            return new SignalSample(lteRsrp, sinr, networkType, SignalFieldOrigin.NetworkLteRsrp, SourceFallback: false);
        }

        return new SignalSample(
            MetricSample.Empty(deviceId, now, SampleSource.DeviceApi, "dBm"),
            sinr,
            networkType,
            SignalFieldOrigin.None,
            SourceFallback: false);
    }
}

public enum NetworkType
{
    Unknown,
    Lte,
    Nsa,
    Sa
}

public enum SignalFieldOrigin
{
    None,
    Z5gRsrp,
    NetworkLteRsrp
}
