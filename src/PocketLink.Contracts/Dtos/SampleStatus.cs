namespace PocketLink.Contracts.Dtos;

/// <summary>
/// 样本状态枚举，对应 PRD 6.4 数据状态字典。
/// </summary>
public enum SampleStatus
{
    Valid,
    Empty,
    Unsupported,
    LoginRequired,
    ReadFailed,
    Stale
}
