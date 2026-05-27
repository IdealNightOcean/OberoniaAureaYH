namespace OberoniaAurea;

public enum InteractionOutcome
{
    /// <summary>大失败</summary>
    CriticalFailure,
    /// <summary>失败</summary>
    Failure,
    /// <summary>普通</summary>
    Neutral,
    /// <summary>成功</summary>
    Success,
    /// <summary>大成功</summary>
    CriticalSuccess
}