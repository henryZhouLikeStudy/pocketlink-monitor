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

            ThroughputHistory.Add(snapshot.Throughput);
            while (ThroughputHistory.Count > 120)
            {
                ThroughputHistory.RemoveAt(0);
            }

            _downloadPoints.Add(snapshot.Throughput.Download.Status == SampleStatus.Valid
                ? snapshot.Throughput.Download.NormalizedValue
                : null);
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
