using System.Collections.ObjectModel;
using PocketLink.Contracts.Dtos;
using PocketLink.Contracts.Interfaces;

namespace PocketLink.Infrastructure.Sampling;

/// <summary>
/// 简单吞吐历史缓冲区：保留固定窗口内的样本，供图表首次渲染绑定。
/// 不做插值伪造；调用方按需求处理空档展示。
/// </summary>
public sealed class ThroughputHistoryBuffer
{
    private readonly int _capacity;
    private readonly Queue<ThroughputSample> _samples = new();

    public ThroughputHistoryBuffer(int capacity = 120)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _capacity = capacity;
    }

    public IReadOnlyCollection<ThroughputSample> Samples => new ReadOnlyCollection<ThroughputSample>(_samples.ToList());

    public void Add(ThroughputSample sample)
    {
        _samples.Enqueue(sample);
        while (_samples.Count > _capacity)
        {
            _samples.Dequeue();
        }
    }
}

/// <summary>
/// 拉取快照并维护吞吐历史的轻量采样器，供 App 层 ViewModel 消费。
/// 首版采样器不实现降档/退避调度（PRD 8.1），仅提供单次拉取和历史追加，
/// 真实调度器留待 SDK 接入阶段实现。
/// </summary>
public sealed class DeviceSampler
{
    private readonly IDeviceAdapter _adapter;
    private readonly ThroughputHistoryBuffer _history;

    public DeviceSampler(IDeviceAdapter adapter, ThroughputHistoryBuffer? history = null)
    {
        _adapter = adapter;
        _history = history ?? new ThroughputHistoryBuffer();
    }

    public IReadOnlyCollection<ThroughputSample> ThroughputHistory => _history.Samples;

    public async Task<DeviceSnapshot> SampleOnceAsync(CancellationToken cancellationToken)
    {
        var snapshot = await _adapter.ReadSnapshotAsync(cancellationToken).ConfigureAwait(false);
        _history.Add(snapshot.Throughput);
        return snapshot;
    }
}
