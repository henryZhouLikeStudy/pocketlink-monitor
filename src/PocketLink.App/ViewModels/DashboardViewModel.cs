using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using PocketLink.Contracts.Dtos;
using PocketLink.Infrastructure.Sampling;

namespace PocketLink.App.ViewModels;

/// <summary>
/// 仪表盘视图模型：绑定连接摘要、信号卡片、吞吐历史和事件条。
/// 采样定时器为 UI 演示用途，PRD 8.1 中定义的降档调度器留待正式 SDK 接入阶段实现。
/// </summary>
public sealed partial class DashboardViewModel : ObservableObject, IDisposable
{
    private readonly DeviceSampler _sampler;
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private ConnectionSummary? _connection;

    [ObservableProperty]
    private SignalSample? _signal;

    [ObservableProperty]
    private ThroughputSample? _latestThroughput;

    [ObservableProperty]
    private string? _lastError;

    private double _peakDownload;
    private double _totalDownload;
    private int _validSampleCount;
    private DateTime _lastUpdateTime = DateTime.UtcNow;

    public string CurrentDownloadText => LatestThroughput?.Download.Status == SampleStatus.Valid
        ? $"{LatestThroughput.Download.NormalizedValue:F2}"
        : "未知";

    public string CurrentUploadText => LatestThroughput?.Upload.Status == SampleStatus.Valid
        ? $"{LatestThroughput.Upload.NormalizedValue:F2}"
        : "未知";

    public string PeakDownloadText => _peakDownload > 0
        ? $"{_peakDownload:F2}"
        : "未知";

    public string AverageDownloadText => _validSampleCount > 0
        ? $"{_totalDownload / _validSampleCount:F2}"
        : "未知";

    public string SampleCountText => ThroughputHistory.Count.ToString();

    public string SignalRsrpText => Signal?.Rsrp.Status == SampleStatus.Valid
        ? $"{Signal.Rsrp.NormalizedValue:F1} {Signal.Rsrp.Unit}"
        : "未知";

    public string SignalRsrpSourceText => Signal?.RsrpOrigin switch
    {
        SignalFieldOrigin.Z5gRsrp => "5G SA (Z5g_rsrp)",
        SignalFieldOrigin.NetworkLteRsrp => "LTE (network_lte_rsrp)",
        _ => "未知",
    };

    public string SignalSinrText => Signal?.Sinr?.Status == SampleStatus.Valid
        ? $"{Signal.Sinr.NormalizedValue:F1} {Signal.Sinr.Unit}"
        : "未知";

    public string NetworkTypeText => Connection is null || Connection.NetworkType == NetworkType.Unknown
        ? "未知"
        : Connection.NetworkType.ToString();

    public string PppStatusText => Connection?.PppStatus ?? "未知";

    public string BatteryText => Connection?.BatteryPercent?.Status == SampleStatus.Valid
        ? $"{Connection.BatteryPercent.NormalizedValue:F0}%"
        : "未知";

    public string ModelText => Connection?.ModelName ?? "未知";

    public string FirmwareText => Connection?.FirmwareVersion ?? "未知";

    public string LastUpdatedText => _lastUpdateTime.ToString("HH:mm:ss");

    public string UnitText => "单位待验证";

    public ObservableCollection<ThroughputSample> ThroughputHistory { get; } = new();

    public ObservableCollection<EventEntry> RecentEvents { get; } = new();

    private readonly ObservableCollection<double?> _downloadPoints = new();
    private readonly ObservableCollection<double?> _uploadPoints = new();

    public ISeries[] ThroughputSeries { get; }

    public DashboardViewModel(DeviceSampler sampler)
    {
        _sampler = sampler;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _timer.Tick += async (_, _) => await TickAsync().ConfigureAwait(true);

        ThroughputSeries = new ISeries[]
        {
            new LineSeries<double?> { Values = _downloadPoints, Name = "下行（单位待验证）" },
            new LineSeries<double?> { Values = _uploadPoints, Name = "上行（单位待验证）" },
        };
    }

    public void StartSampling() => _timer.Start();

    public void StopSampling() => _timer.Stop();

    private async Task TickAsync()
    {
        try
        {
            var snapshot = await _sampler.SampleOnceAsync(CancellationToken.None).ConfigureAwait(true);
            Connection = snapshot.Connection;
            Signal = snapshot.Signal;
            LatestThroughput = snapshot.Throughput;
            LastError = null;
            _lastUpdateTime = DateTime.UtcNow;

            ThroughputHistory.Add(snapshot.Throughput);
            while (ThroughputHistory.Count > 120)
            {
                ThroughputHistory.RemoveAt(0);
            }

            var downloadValue = snapshot.Throughput.Download.Status == SampleStatus.Valid
                ? snapshot.Throughput.Download.NormalizedValue
                : (double?)null;

            if (downloadValue.HasValue)
            {
                if (downloadValue.Value > _peakDownload)
                {
                    _peakDownload = downloadValue.Value;
                }
                _totalDownload += downloadValue.Value;
                _validSampleCount++;
            }

            _downloadPoints.Add(downloadValue);
            _uploadPoints.Add(snapshot.Throughput.Upload.Status == SampleStatus.Valid
                ? snapshot.Throughput.Upload.NormalizedValue
                : null);
            while (_downloadPoints.Count > 120)
            {
                _downloadPoints.RemoveAt(0);
            }
            while (_uploadPoints.Count > 120)
            {
                _uploadPoints.RemoveAt(0);
            }

            foreach (var evt in snapshot.RecentEvents)
            {
                RecentEvents.Insert(0, evt);
            }

            while (RecentEvents.Count > 50)
            {
                RecentEvents.RemoveAt(RecentEvents.Count - 1);
            }

            OnPropertyChanged(nameof(CurrentDownloadText));
            OnPropertyChanged(nameof(CurrentUploadText));
            OnPropertyChanged(nameof(PeakDownloadText));
            OnPropertyChanged(nameof(AverageDownloadText));
            OnPropertyChanged(nameof(SampleCountText));
            OnPropertyChanged(nameof(SignalRsrpText));
            OnPropertyChanged(nameof(SignalRsrpSourceText));
            OnPropertyChanged(nameof(SignalSinrText));
            OnPropertyChanged(nameof(NetworkTypeText));
            OnPropertyChanged(nameof(PppStatusText));
            OnPropertyChanged(nameof(BatteryText));
            OnPropertyChanged(nameof(ModelText));
            OnPropertyChanged(nameof(FirmwareText));
            OnPropertyChanged(nameof(LastUpdatedText));
        }
        catch (Exception ex)
        {
            LastError = $"采样失败：{ex.Message}";
        }
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
